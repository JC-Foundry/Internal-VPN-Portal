using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder.Extensions;
using VPN_Portal.Extensions;
using VPN_Portal.Models;
using VPN_Portal.Services;

namespace VPN_Portal.Pages;

public class Download : PageModel
{
    private readonly DownloadTokenService _downloadTokenService;

    public Download(DownloadTokenService downloadTokenService)
    {
        _downloadTokenService = downloadTokenService;
    }
    
    public string QrSvg { get; set; }
    public string? ErrorTitle { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsError => !string.IsNullOrEmpty(ErrorMessage);
    
    public async Task<IActionResult> OnGet(string tokenId, DownloadTokenPurpose purpose = DownloadTokenPurpose.Config)
    {
        var result = await _downloadTokenService.RedeemToken(tokenId, purpose);
        switch (result.Outcome)
        {
            case DownloadOutcome.NotFound:
                return NotFound();
            case DownloadOutcome.Success when result.Purpose == DownloadTokenPurpose.Config:
                return File(result.ConfigBytes!, "text/plain", result.FileName);
            case DownloadOutcome.Success:
                QrSvg = _downloadTokenService.GenerateQrCode(result.Config!);
                return Page();
        }

        ErrorTitle = result.Outcome.ToSpacedString();
        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}