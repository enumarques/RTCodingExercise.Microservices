namespace Catalog.Domain
{
    public class PaginatedPlates
    {
        public IEnumerable<Plate> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string? SortField {get;set;}
        public SortOrder SortOrder {get;set;} = SortOrder.Unspecified;

        public PaginatedPlates(IEnumerable<Plate> plates, int totalPlates, int platesPerPage, int pageIndex, string? sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            Items = plates;
            TotalCount = totalPlates;
            PageSize = platesPerPage;
            PageIndex = pageIndex;
            SortField = sortField;
            SortOrder = sortOrder;
        }

        public PaginatedPlates() {
            Items = new List<Plate>();
        }
    }
}