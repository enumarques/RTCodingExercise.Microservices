using WebMVC.Services;
using System.Linq;

namespace WebMVC.Models
{
    public class PlateRepository: IPlateRepository
    {
        private ILogger<PlateRepository> _logger {get;set;}
        private IPlateServiceClient _plateService {get;set;}

        public PlateRepository(IPlateServiceClient plateService, ILogger<PlateRepository> logger)
        {
            _plateService = plateService;
            _logger = logger;
        }

        public async Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync( int pageSize = 20, int pageIndex = 0, string? sortField = null, SortOrder sortOrder = SortOrder.Unspecified )
        {
            _logger.LogInformation("Retrieving plates from the plate service");

            return await _plateService.GetPlatesAsync(pageSize, pageIndex, sortField, sortOrder);
        }

        public async Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync( PlateListRequest request )
        {
            _logger.LogInformation("Retrieving plates from the plate service");

            return await _plateService.GetPlatesAsync(request.PageSize, request.PageIndex, request.SortField, request.SortOrder, request.Filter);
        }

        public async Task<bool> AddPlateAsync(
            decimal purchasePrice,
            decimal salePrice,
            string registration
        )
        {
            // enforce business rules

            // determine letters and numbers for registration
            var letters = "";
            var numbers = 0;

            return await _plateService.AddPlateAsync(new Plate() {
                Id = Guid.NewGuid(),
                Registration = registration,
                PurchasePrice = purchasePrice,
                SalePrice = salePrice,
                Letters = letters,
                Numbers = numbers
            });
        }
    }

}