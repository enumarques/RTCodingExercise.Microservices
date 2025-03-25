namespace Catalog.API.Models
{
    public interface IPlateRepository
    {
        public PaginatedPlates GetPlates(int pageIndex, int pageSize);
        public PaginatedPlates GetPlates(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder);
        public PaginatedPlates GetPlates(int pageIndex, int pageSize, string? sortField, SortOrder sortOrder, SearchFilters filters);

        public Result<Plate> GetPlate(Guid id);

        public Result<Plate> AddPlate(Guid id, Plate plateData);

        public Result<Plate> ReservePlate(Guid plateId);
        public Result<Plate> RemoveReservation(Guid plateId);
    }
}