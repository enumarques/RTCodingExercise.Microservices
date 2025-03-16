namespace Catalog.Domain
{
    public class PaginatedPlates
    {
        public IEnumerable<Plate> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        public PaginatedPlates(IEnumerable<Plate> plates, int totalPlates, int platesPerPage, int pageIndex)
        {
            Items = plates;
            TotalCount = totalPlates;
            PageSize = platesPerPage;
            PageIndex = pageIndex;
        }

        public PaginatedPlates() {
            Items = new List<Plate>();
        }
    }
}