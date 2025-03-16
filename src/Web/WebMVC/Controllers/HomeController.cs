using RTCodingExercise.Microservices.Models;
using System.Diagnostics;
using WebMVC.Models;

namespace RTCodingExercise.Microservices.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlateRepository _plateRepository;

        public HomeController(IPlateRepository plateRepository, ILogger<HomeController> logger)
        {
            _plateRepository = plateRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageIndex = 0)
        {
            _logger.LogInformation("Page number {PageNumber} requested", pageIndex + 1);
            var page = await _plateRepository.GetPlatesAsync(pageIndex: pageIndex);

            return View(page);
        }

        [HttpGet]
        [Route("/AddPlate")]
        public IActionResult AddPlate(string? registration, decimal? purchasePrice, decimal? salePrice)
        {
            return View((registration??string.Empty, purchasePrice??0m, salePrice??0m));
        }

        [HttpPost]
        [Route("/AddPlate")]
        public async Task<IActionResult> AddPlate(string registration, decimal purchasePrice, decimal salePrice)
        {
            if (ModelState.IsValid)
            {
                var result = await _plateRepository.AddPlateAsync(purchasePrice, salePrice, registration);
                if (result) return RedirectToAction(nameof(Index));
            }

            return View((registration, purchasePrice, salePrice));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}