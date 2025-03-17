using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Catalog.API.Models
{
    public class PlateRepository : IPlateRepository
    {
        private ApplicationDbContext InventoryContext {get; set;}
        private ILogger<PlateRepository> _logger {get;}
        
        public PlateRepository(ApplicationDbContext context, ILogger<PlateRepository> logger)
        {
            InventoryContext = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<PaginatedPlates> GetPlatesAsync(
            int pageIndex = 0,
            int pageSize = 20
        )
        {
            return await GetPlatesAsync( pageIndex, pageSize, null, SortOrder.Unspecified);
        }

        public async Task<PaginatedPlates> GetPlatesAsync(
            int pageIndex = 0,
            int pageSize = 20,
            string? sortField = null,
            SortOrder sortOrder = SortOrder.Unspecified
        )
        {
            var totalPlates = await InventoryContext.Plates.CountAsync();
            var plateListQuery = InventoryContext.Plates.AsQueryable<Plate>();

            _logger.LogInformation("Adding sort by {SortField} in {SortOrder}", sortField, sortOrder);
            plateListQuery = AddOrderClauseToQuery( plateListQuery, sortField, sortOrder );

            var plateList = await plateListQuery.Skip(pageSize * pageIndex)
                            .Take(pageSize)
                            .ToListAsync();

            _logger.LogInformation("Retrieved {ResultCount} plates of {TotalPlates} from page {PageIndex}", plateList.Count, totalPlates, pageIndex);
            return new PaginatedPlates(plateList, totalPlates, pageSize, pageIndex, sortField, sortOrder);
        }

        public async Task<Result<Plate>> AddPlateAsync(Guid Id, Plate plateData)
        {
            var validationResult = ValidatePlate(Id, plateData);
            if (validationResult != ValidationResult.Success)
            {
                _logger.LogError("Plate validation failed, {ValidationErrors}", validationResult.ErrorMessage);
                return new Result<Plate>(new ValidationException(validationResult.ErrorMessage));
            }


            var existingPlate = InventoryContext.Plates.Where( p => p.Registration == plateData.Registration )
                .Select(p => p.Id)
                .FirstOrDefault();
            if (existingPlate != Guid.Empty)
            {
                _logger.LogError("Could not insert plate {Registration}, already present", plateData.Registration);
                return new Result<Plate>(new ArgumentException("Registration already exists in store"));
            }

            plateData.Id = Id;
            var result = await InventoryContext.Plates.AddAsync(plateData);
            await InventoryContext.SaveChangesAsync();

            return new Result<Plate>(result.Entity);
        }


        private IQueryable<Plate> AddOrderClauseToQuery(IQueryable<Plate> query, string? sortField, SortOrder sortOrder)
        {
            if (string.IsNullOrEmpty(sortField))
                return query;

            var sortBy = (SortField)Enum.Parse(typeof(SortField), sortField);
            if (sortBy == SortField.None)
                return query;

            Expression<Func<Plate, object>> expression = sortBy == SortField.Price ? plate => plate.SalePrice : plate => plate.Id;
            if (sortOrder == SortOrder.Descending)
            {
                return query.OrderByDescending(expression);
            }

            return query.OrderBy(expression);
        }


        private ValidationResult ValidatePlate(Guid id, Plate data)
        {
            var errors = new List<string>();

            // apply business rules
            if (data.Id != id && data.Id != Guid.Empty)
            {
                errors.Add("Id missing or mismatched");
            }

            if (data.PurchasePrice > data.SalePrice)
            {
                errors.Add("Purchase price lower than sale price");
            }

            _logger.LogTrace("Validated plate, got {ErrorCount} errors", errors.Count);

            return errors.Count == 0? ValidationResult.Success! : new ValidationResult(string.Join(", ", errors));
        }
    }
}