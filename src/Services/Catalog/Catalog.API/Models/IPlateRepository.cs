namespace Catalog.API.Models
{
    public interface IPlateRepository
    {
        public PaginatedPlates GetPlates(int pageIndex, int pageSize);
        public PaginatedPlates GetPlates(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder);
        public PaginatedPlates GetPlates(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder, SearchFilters filters);

        public Result<Plate> AddPlate(Guid id, Plate plateData);
    }
}