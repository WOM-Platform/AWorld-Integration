using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WomAWorldIntegration.Models;

namespace WomAWorldIntegration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            HttpClient http,
            ILogger<IndexModel> logger
        )
        {
            _http = http;
            _logger = logger;
        }

        [BindProperty]
        public UserInfoModel? UserInfo { get; set; }

        public string? Error { get; set; }

        public string? Output { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _logger.LogInformation("Processing username {0}", UserInfo?.Username);

            if(string.IsNullOrWhiteSpace(UserInfo?.Username))
            {
                ModelState.AddModelError(nameof(UserInfo) + "." + nameof(UserInfo.Username), "Username cannot be empty");
            }
            if(!Guid.TryParseExact(UserInfo?.Secret ?? string.Empty, "D", out Guid secretId))
            {
                ModelState.AddModelError(nameof(UserInfo) + "." + nameof(UserInfo.Secret), "Secret is not valid");
                _logger.LogWarning("Could not parse secret as GUID");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var profile = await _http.GetFromJsonAsync<UserProfile>($"https://aworld.org/u/{UserInfo.Username}?format=json");

                // Verification
                if(profile?.Ctx?.Profile?.User?.Sub != secretId)
                {
                    _logger.LogError("Secret ID does not match profile sub");

                    Error = "The secret does not match your profile.";
                    return Page();
                }

                /*
                var sb = new StringBuilder();
                sb.Append($"Profile {profile.Ctx.Username} of {profile.Ctx.Profile.User.FirstName} {profile.Ctx.Profile.User.LastName}.<br />");
                sb.Append("Metrics:<br />");
                foreach (var metric in profile.Ctx.Profile.Metrics)
                {
                    sb.Append($"{metric.Id}, {metric.Name} {metric.Amount}<br />");
                }
                _logger.LogInformation("Results: {0}", sb.ToString());
                Output = sb.ToString();
                */

                Output = "Everything correct";

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process: {0}", ex.Message);

                Error = $"Unable to process user profile ({ex.Message}).";
                return Page();
            }
        }
    }
}
