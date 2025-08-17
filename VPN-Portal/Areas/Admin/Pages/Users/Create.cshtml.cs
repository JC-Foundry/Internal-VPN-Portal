using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Admin.Pages.Users
{
    [Authorize(Roles = SystemRoles.SystemAdmin)]
    public class CreateModel : PageModel
    {
        private readonly AdminService _adminService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(AdminService adminService, ILogger<CreateModel> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        public class UserInputModel
        {
            [Required]
            [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DisplayName("Username")]
            [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, and ._- characters.")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [DisplayName("Email Address")]
            public string Email { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "The {0} must be at max {1} characters long.")]
            [DisplayName("Display Name")]
            public string? DisplayName { get; set; }
        }

        [BindProperty]
        public UserInputModel Input { get; set; } = new();

        [TempData]
        public string? CreatedUsername { get; set; }
        
        [TempData]
        public string? CreatedPassword { get; set; }
        
        public bool ShowSuccess => !string.IsNullOrEmpty(CreatedUsername);

        public void OnGet()
        {
            // Just display the form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Create the user using AdminService
            var (success, password) = await _adminService.TryCreateUser(
                Input.Username, 
                Input.Email, 
                Input.DisplayName
            );

            if (success)
            {
                _logger.LogInformation("Admin created new user: {Username}", Input.Username);
                
                // Store success data in TempData for display
                CreatedUsername = Input.Username;
                CreatedPassword = password;
                
                // Redirect to the same page to show success message
                return RedirectToPage();
            }

            ModelState.AddModelError(string.Empty, "Failed to create user. Please try again.");
            return Page();
        }
    }
}