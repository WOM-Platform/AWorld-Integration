using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
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

            /*
            var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(UserInfo?.Secret ?? string.Empty));
            var parts = decoded.Split(' ');
            if (parts.Length != 2)
            {
                ModelState.AddModelError(nameof(UserInfo.Secret), "Secret could not be decoded");
                _logger.LogWarning("Could not decode secret (has {0} parts)", parts.Length);
            }
            */

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var profile = await _http.GetFromJsonAsync<UserProfile>($"https://aworld.org/u/{UserInfo.Username}?lang=en&format=json");

                // Verification
                /*if (!parts[1].Equals(profile?.Ctx?.Profile?.User?.Sub, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogError("Secret sub does not match profile sub");

                    Error = "Secret sub does not match profile sub.";
                    return Page();
                }*/

                var sb = new StringBuilder();
                sb.Append($"Profile {profile.Ctx.Username} of {profile.Ctx.Profile.User.FirstName} {profile.Ctx.Profile.User.LastName}.<br />");
                sb.Append("Metrics:<br />");
                foreach (var metric in profile.Ctx.Profile.Metrics)
                {
                    sb.Append($"{metric.Id}, {metric.Name} {metric.Amount}<br />");
                }
                _logger.LogInformation("Results: {0}", sb.ToString());
                Output = sb.ToString();

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
