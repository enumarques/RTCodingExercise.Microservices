namespace WebMVC.Models
{
    public class PaginatedItemsServiceResponse<T>
    {
        public IEnumerable<T> Items {get;set;} = new List<T>();
        public int TotalCount {get;set;} = 0;
        public int PageIndex {get;set;} = 0;
        public int PageSize {get;set;} = 0;
    }
}