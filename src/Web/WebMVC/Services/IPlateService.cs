using WebMVC.Models;

namespace WebMVC.Services
{
    public interface IPlateServiceClient
    {
        public Task<PaginatedItemsServiceResponse<Plate>> GetPlatesAsync(int pageSize, int pageIndex);

        public Task<bool> AddPlateAsync(Plate newPlate);
    }
}