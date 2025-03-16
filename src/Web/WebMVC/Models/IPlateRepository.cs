namespace WebMVC.Models
{
    public interface IPlateRepository
    {
        public Task<PaginatedPlatesViewModel> GetPlatesAsync( int pageSize = 20, int pageIndex = 0 );

        public Task<bool> AddPlateAsync(
            decimal purchasePrice,
            decimal salePrice,
            string registration
        );
    }
}