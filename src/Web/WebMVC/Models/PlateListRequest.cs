namespace WebMVC.Models
{
    public class PlateListRequest
    {
        public int PageSize = 20;
        public int PageIndex =0;
        public string? SortField = null;
        public SortOrder SortOrder = SortOrder.Unspecified;
        public PlateFilter? Filter = null;
    }
}