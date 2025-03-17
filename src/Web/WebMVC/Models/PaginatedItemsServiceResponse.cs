namespace WebMVC.Models
{
    public class PaginatedItemsServiceResponse<T>
    {
        public IEnumerable<T> Items {get;set;} = new List<T>();
        public int TotalCount {get;set;} = 0;
        public int PageIndex {get;set;} = 0;
        public int PageSize {get;set;} = 0;
        public string? SortField {get;set;} = null;
        public SortOrder SortOrder {get;set;} = SortOrder.Unspecified;
        public PlateFilter? Filter {get;set;} = null;
    }
}