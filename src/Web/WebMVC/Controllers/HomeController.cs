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

        public async Task<IActionResult> Index(int pageIndex = 0, string? sortField = null, SortOrder sortOrder = SortOrder.Unspecified, string? letterFilter = null, string? numberFilter = null)
        {
            var filter = new PlateFilter();
            if (!string.IsNullOrEmpty(letterFilter)) filter.Letters = letterFilter;
            if (!string.IsNullOrEmpty(numberFilter)) filter.Numbers = numberFilter;

            _logger.LogInformation("Page number {PageNumber} requested", pageIndex + 1);
            var request = new PlateListRequest()
            {
                PageIndex = pageIndex,
                SortField = sortField,
                SortOrder = sortOrder,
                Filter = filter
            };

            var storedPlates = await _plateRepository.GetPlatesAsync(request);

            var displayPlates = new PaginatedPlatesViewModel(
                storedPlates.Items.Select( i => {i.SalePrice *= MarkupMultiplier; return i;} ).ToList(),
                storedPlates.PageIndex,
                storedPlates.PageIndex > 0? storedPlates.PageIndex - 1: null,
                storedPlates.TotalCount > storedPlates.PageIndex * storedPlates.PageSize? storedPlates.PageIndex + 1: null,
                storedPlates.TotalCount,
                storedPlates.SortField,
                storedPlates.SortOrder,
                storedPlates.Filter 
            );

            var qS = PagelessQuery(sortField, sortOrder, filter);
            var lastPageIndex = Math.Ceiling((double)(storedPlates.TotalCount / storedPlates.PageSize));
            var firstQs = qS.Add("PageIndex", "0");
            var lastQs = qS.Add("PageIndex", lastPageIndex.ToString());
            var prevQs = qS.Add("PageIndex", (storedPlates.PageIndex - 1).ToString());
            var nextQs = qS.Add("PageIndex", (storedPlates.PageIndex + 1).ToString());
            ViewData["FirstPageLink"] = firstQs.ToUriComponent();
            ViewData["LastPageLink"] = lastQs.ToUriComponent();
            if (lastPageIndex > pageIndex) ViewData["NextPageLink"] = nextQs.ToUriComponent();
            if (pageIndex > 0) ViewData["PrevPageLink"] = prevQs.ToUriComponent();

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

        private static QueryString PagelessQuery(string? sortField = null, SortOrder? sortOrder = null, PlateFilter? filter = null)
        {
            var queryParams = new Dictionary<string, string?>();
            if (! string.IsNullOrEmpty(sortField))
            {
                queryParams.Add("sortField", sortField);
                queryParams.Add("sortOrder", sortOrder.ToString());
            }
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Numbers)) queryParams.Add("numberFilter", filter.Numbers);
                if (!string.IsNullOrEmpty(filter.Letters)) queryParams.Add("letterFilter", filter.Letters);
            }

            return QueryString.Create(queryParams);
        }
    }
}