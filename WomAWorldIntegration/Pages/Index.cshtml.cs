using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using WomAWorldIntegration.Models;
using WomPlatform.Connector;
using WomPlatform.Connector.Models;

namespace WomAWorldIntegration.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly MongoDatabase _mongo;
        private readonly Instrument _womInstrument;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            HttpClient http,
            MongoDatabase mongo,
            Instrument womInstrument,
            ILogger<IndexModel> logger
        )
        {
            _http = http;
            _mongo = mongo;
            _womInstrument = womInstrument;
            _logger = logger;
        }

        [BindProperty]
        public UserInfoModel? UserInfo { get; set; }

        public string? Error { get; set; }

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

            _logger.LogInformation($"Processing profile of {UserInfo.Username} for reward");

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

                bool newProfilePrize = false;
                var existingUser = await _mongo.GetUserByUsername(UserInfo.Username);
                if(existingUser == null)
                {
                    newProfilePrize = true;
                    existingUser = new Documents.User
                    {
                        Id = ObjectId.GenerateNewId(),
                        Username = UserInfo.Username,
                        Sub = secretId,
                        CreatedOn = DateTime.UtcNow,
                    };
                }
                _logger.LogDebug("Profile loaded, new profile reward {0}", newProfilePrize);

                // Update profile
                int previousLevelIndex = existingUser.CurrentLevelIndex;

                existingUser.SavedCo2 = profile.Ctx.Profile.Metrics.Where(m => m.Id == 2).Single().Amount;
                existingUser.SavedWater = profile.Ctx.Profile.Metrics.Where(m => m.Id == 1).Single().Amount;
                existingUser.SavedEnergy = profile.Ctx.Profile.Metrics.Where(m => m.Id == 4).Single().Amount;
                existingUser.ActsOfLove = profile.Ctx.Profile.Metrics.Where(m => m.Id == 5).Single().Amount;
                existingUser.ActionsCount = profile.Ctx.Profile.Actions;
                existingUser.CurrentLevelIndex = profile.Ctx.Profile.Level.Index;

                _logger.LogDebug("Current level of user {0}, previous level {1}", existingUser.CurrentLevelIndex, previousLevelIndex);

                await _mongo.ReplaceUserByUsername(existingUser);

                // Process reward
                var amountOfVouchers = GetVoucherCount(newProfilePrize, previousLevelIndex, existingUser.CurrentLevelIndex);
                
                VoucherRequest response = null;
                if(amountOfVouchers > 0)
                {
                    response = await _womInstrument.RequestVouchers(new VoucherCreatePayload.VoucherInfo[]
                    {
                        new VoucherCreatePayload.VoucherInfo
                        {
                            Aim = "N",
                            Count = amountOfVouchers,
                            CreationMode = VoucherCreatePayload.VoucherCreationMode.SetLocationOnRedeem,
                            Timestamp = DateTime.Now,
                        }
                    });
                }
                
                await _mongo.RegisterPrize(new Documents.Prize
                {
                    UserId = existingUser.Id,
                    Username = existingUser.Username,
                    CreatedOn = DateTime.UtcNow,
                    NewRegistrationPrize = newProfilePrize,
                    AmountOfLevelsGained = previousLevelIndex,
                    AmountOfVouchers = amountOfVouchers,
                    WomUrl = response?.Link,
                    WomPassword = response?.Password,
                });

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process: {0}", ex.Message);

                Error = $"Unable to process user profile ({ex.Message}).";
                return Page();
            }
        }

        private int GetLeavesForLevel(int level)
        {
            return level switch
            {
                1  =>       0,
                2  =>     150,
                3  =>    1040,
                4  =>    4000,
                5  =>    8000,
                6  =>   12000,
                7  =>   50000,
                8  =>  100000,
                9  =>  250000,
                10 => 1000000,
                11 => 5000000,
                _  =>       0,
            };
        }

        private int GetVoucherCount(bool newProfilePrize, int previousLevel, int currentLevel)
        {
            return
                (newProfilePrize ? 10 : 0) + // Bonus 10 vouchers for new profile
                (int)Math.Ceiling((GetLeavesForLevel(currentLevel) - GetLeavesForLevel(previousLevel)) / 50.0) // 1 voucher per 50 additional leaves
            ;
        }
    }
}
