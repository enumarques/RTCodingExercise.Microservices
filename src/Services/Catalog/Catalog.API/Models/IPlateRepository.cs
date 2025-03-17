namespace Catalog.API.Models
{
    public interface IPlateRepository
    {
        public Task<PaginatedPlates> GetPlatesAsync(int pageIndex, int pageSize);
        public Task<PaginatedPlates> GetPlatesAsync(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder);
        public Task<PaginatedPlates> GetPlatesAsync(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder, SearchFilters filters);

        public Task<Result<Plate>> AddPlateAsync(Guid id, Plate plateData);
    }
}