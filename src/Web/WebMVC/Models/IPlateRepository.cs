namespace WebMVC.Models
{
    public interface IPlateRepository
    {
        public Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync(
            int pageSize = 20,
            int pageIndex = 0,
            string? sortField = null,
            SortOrder sortOrder = SortOrder.Unspecified
        );

        public Task<bool> AddPlateAsync(
            decimal purchasePrice,
            decimal salePrice,
            string registration
        );
    }
}