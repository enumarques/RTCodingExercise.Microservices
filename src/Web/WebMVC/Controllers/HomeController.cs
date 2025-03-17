using RTCodingExercise.Microservices.Models;
using System.Diagnostics;
using WebMVC.Models;

namespace RTCodingExercise.Microservices.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPlateRepository _plateRepository;
        const decimal MarkupMultiplier = 1.2m;


        public HomeController(IPlateRepository plateRepository, ILogger<HomeController> logger)
        {
            _plateRepository = plateRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageIndex = 0, string? sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            _logger.LogInformation("Page number {PageNumber} requested", pageIndex + 1);
            var storedPlates = await _plateRepository.GetPlatesAsync(pageIndex: pageIndex, sortField: sortField, sortOrder: sortOrder);

            var displayPlates = new PaginatedPlatesViewModel(
                storedPlates.Items.Select( i => {i.SalePrice *= MarkupMultiplier; return i;} ).ToList(),
                storedPlates.PageIndex,
                storedPlates.PageIndex > 0? storedPlates.PageIndex - 1: null,
                storedPlates.TotalCount > storedPlates.PageIndex * storedPlates.PageSize? storedPlates.PageIndex + 1: null,
                storedPlates.TotalCount,
                storedPlates.SortField,
                storedPlates.SortOrder
            );

            return View(displayPlates);
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