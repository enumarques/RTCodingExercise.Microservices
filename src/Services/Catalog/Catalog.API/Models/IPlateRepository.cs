namespace Catalog.API.Models
{
    public interface IPlateRepository
    {
        public Task<PaginatedPlates> GetPlatesAsync(int PageIndex, int PageSize);

        public Task<Result<Plate>> AddPlateAsync(Guid id, Plate plateData);
    }
}