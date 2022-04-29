using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WomAWorldIntegration.Pages
{
    public class ShowPrizeModel : PageModel
    {
        private readonly MongoDatabase _mongo;
        private readonly ILogger<ShowPrizeModel> _logger;

        public ShowPrizeModel(
            MongoDatabase mongo,
            ILogger<ShowPrizeModel> logger
        )
        {
            _mongo = mongo;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string Pid { get; set; }

        public int AmountOfVouchers { get; set; }

        public string WomUrl { get; set; }

        public string WomPassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var prize = await _mongo.GetPrizeById(Pid);
            if(prize == null)
            {
                return NotFound();
            }

            AmountOfVouchers = prize.AmountOfVouchers;
            WomUrl = prize.WomUrl;
            WomPassword = prize.WomPassword;

            return Page();
        }
    }
}
