using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Helpers;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class EventPageModel<T> : PageModel
    where T : class, ISecurityEvent
{
    protected readonly SecurityService SecurityService;
    protected readonly SecurityActionService SecurityActionService;

    public EventPageModel(SecurityService securityService,
        SecurityActionService securityActionService)
    {
        SecurityService = securityService;
        SecurityActionService = securityActionService;
    }
    
    public SecurityEvent BaseEvent { get; set; }  
    public T  SecurityEvent { get; set; }
    public List<SecurityAction> SecurityActions { get; set; }

    protected async Task<bool> SetupPage(string eventId)
    {
        var baseEvent = await SecurityService.GetSecurityEvent(eventId);
        if (baseEvent == null) return false;

        var specificEvent = await SecurityService.GetSpecificSecurityEvent<T>(eventId);
        if(specificEvent == null) return false;

        BaseEvent = baseEvent;
        SecurityEvent = specificEvent;
        SecurityActions = baseEvent.Actions.ToList();
        return true;
    }

    public async Task<IActionResult> OnGet(string eventId)
    {
        var res = await SetupPage(eventId);
        return res ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostExecuteAction([FromBody] ExecuteActionRequest request)
    {
        try
        {
            // Load the event data
            var loaded = await SetupPage(request.EventId);
            if (!loaded || BaseEvent?.UserId == null)
            {
                return new JsonResult(new { success = false, message = "Security event or user not found" });
            }

            var userId = BaseEvent.UserId;
            var result = false;

            // Execute the appropriate action based on actionType
            switch (request.ActionType)
            {
                case ActionType.AccountDisabled:
                    result = await SecurityActionService.PerformAccountDisable(userId, request.EventId, TakenByType.Administrator);
                    break;

                case ActionType.AccountEnabled:
                    result = await SecurityActionService.PerformAccountEnable(userId, request.EventId, TakenByType.Administrator);
                    break;

                case ActionType.AccountDeleted:
                    result = await SecurityActionService.PerformAccountDeletion(userId, request.EventId, TakenByType.Administrator);
                    break;

                case ActionType.RouterPeerRemoved:
                {
                    var peerId = SecurityEventHelper.TryGetPeerId(SecurityEvent);
                    if (peerId == null)
                        return new JsonResult(new { success = false, message = "PeerId not found for this event" });

                    result = await SecurityActionService.PerformRouterPeerRemoved(peerId, userId, TakenByType.Administrator, saveNow: true);
                    break;
                }

                case ActionType.PeerSoftRemoved:
                {
                    var peerIdString = SecurityEventHelper.TryGetPeerId(SecurityEvent);
                    if (peerIdString == null)
                        return new JsonResult(new { success = false, message = "PeerId not found for this event" });

                    // Handle PeerAbuse with multiple peers
                    var peerIds = peerIdString.Contains(PeerAbuseEvent.Delimiter)
                        ? peerIdString.Split(PeerAbuseEvent.Delimiter, StringSplitOptions.RemoveEmptyEntries)
                        : [peerIdString];

                    foreach (var peerId in peerIds)
                    {
                        result = await SecurityActionService.PerformPeerSoftDelete(peerId, userId, request.EventId, TakenByType.Administrator);
                        if (!result) break;
                    }
                    break;
                }

                case ActionType.PeerRestored:
                {
                    var peerIdString = SecurityEventHelper.TryGetPeerId(SecurityEvent);
                    if (peerIdString == null)
                        return new JsonResult(new { success = false, message = "PeerId not found for this event" });

                    // Handle PeerAbuse with multiple peers
                    var peerIds = peerIdString.Contains(PeerAbuseEvent.Delimiter)
                        ? peerIdString.Split(PeerAbuseEvent.Delimiter, StringSplitOptions.RemoveEmptyEntries)
                        : [peerIdString];

                    foreach (var peerId in peerIds)
                    {
                        result = await SecurityActionService.PerformPeerRestore(peerId, userId, request.EventId, TakenByType.Administrator);
                        if (!result) break;
                    }
                    break;
                }

                case ActionType.PeerHardRemoved:
                {
                    var peerIdString = SecurityEventHelper.TryGetPeerId(SecurityEvent);
                    if (peerIdString == null)
                        return new JsonResult(new { success = false, message = "PeerId not found for this event" });

                    // Handle PeerAbuse with multiple peers
                    var peerIds = peerIdString.Contains(PeerAbuseEvent.Delimiter)
                        ? peerIdString.Split(PeerAbuseEvent.Delimiter, StringSplitOptions.RemoveEmptyEntries)
                        : new[] { peerIdString };

                    foreach (var peerId in peerIds)
                    {
                        result = await SecurityActionService.PerformPeerHardDelete(peerId, userId, request.EventId, TakenByType.Administrator);
                        if (!result) break;
                    }
                    break;
                }

                default:
                    return new JsonResult(new { success = false, message = "Unknown action type" });
            }

            return result 
                ? new JsonResult(new { success = true, message = "Action executed successfully" }) 
                : new JsonResult(new { success = false, message = "Failed to execute action" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    public async Task<IActionResult> OnPostChangeStatus([FromBody] ChangeStatusRequest request)
    {
        try
        {
            var result = await SecurityService.TryUpdateSecurityEventStatus(request.EventId, request.NewStatus);
            return result
                ? new JsonResult(new { success = true, message = "Status updated successfully" })
                : new JsonResult(new { success = false, message = "Failed to update status - event not found" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    public class ExecuteActionRequest
    {
        public string EventId { get; set; }
        public ActionType ActionType { get; set; }
    }

    public class ChangeStatusRequest
    {
        public string EventId { get; set; }
        public EventStatus NewStatus { get; set; }
    }
}