using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Web.ApiAccess;

namespace Web.Pages {

    public class IndexModel : PageModel {

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() {

        }

        [BindProperty]
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [BindProperty]
        [Required]
        public string Secret { get; set; }

        public string Result { get; set; }

        public async Task<IActionResult> OnPostAsync() {
            if(!ModelState.IsValid) {
                return Page();
            }

            _logger.LogInformation("Processing username {0}", Username);

            var decoded = Encoding.ASCII.GetString(Convert.FromBase64String(Secret));
            var parts = decoded.Split(' ');
            if(parts.Length != 2) {
                ModelState.AddModelError(nameof(Secret), $"Secret could not be decoded ({parts.Length} parts)");
                _logger.LogWarning("Could not decode secret into two parts");
            }

            if(!ModelState.IsValid) {
                return Page();
            }

            try {
                var profile = await Startup.Client.GetFromJsonAsync<Profile>($"https://aworld.org/u/{Username}?lang=en&format=json");

                // Verification
                if(!parts[1].Equals(profile.Ctx.Profile.User.Sub, StringComparison.InvariantCultureIgnoreCase)) {
                    _logger.LogError("Secret sub does not match profile sub");

                    ViewData["Error"] = "Secret sub does not match profile sub.";
                    return Page();
                }

                /*var token = new JsonWebToken(profile.Ctx.Profile.User.Signature);
                if(!parts[1].Equals(token.Subject, StringComparison.InvariantCultureIgnoreCase)) {
                    _logger.LogError("Secret sub does not match profile signature");

                    ViewData["Error"] = "Secret sub does not match profile signature.";
                    return Page();
                }*/

                var sb = new StringBuilder();
                sb.Append($"Profile {profile.Ctx.Username} of {profile.Ctx.Profile.User.FirstName} {profile.Ctx.Profile.User.LastName}.<br />");
                sb.Append("Metrics:<br />");
                foreach(var metric in profile.Ctx.Profile.Metrics) {
                    sb.Append($"{metric.Id}, {metric.Name} {metric.Amount}<br />");
                }
                Result = sb.ToString();

                return Page();
            }
            catch(Exception ex) {
                _logger.LogError(ex, "Unable to process: {0}", ex.Message);

                ViewData["Error"] = $"Unable to fetch user profile ({ex.Message}).";
                return Page();
            }
        }

    }

}
