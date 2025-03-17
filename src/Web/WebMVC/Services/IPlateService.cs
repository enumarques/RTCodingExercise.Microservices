using WebMVC.Models;

namespace WebMVC.Services
{
    public interface IPlateServiceClient
    {
        public Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync(int pageSize, int pageIndex, string? sortField, SortOrder sortOrder, PlateFilter? filter = null);

        public Task<bool> AddPlateAsync(Plate newPlate);
    }
}