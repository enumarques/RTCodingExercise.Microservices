namespace WebMVC.Models
{
    public class PaginatedPlatesViewModel
    {
        public IEnumerable<Plate> Plates {get;}
        public int CurrentPage {get;}
        public int? NextPage {get;}
        public int? PreviousPage {get;}
        public int TotalPlates { get; }
        public string? SortField {get;}
        public SortOrder SortOrder {get;} = SortOrder.Unspecified;

        public PlateFilter Filter { get; set; } = new PlateFilter();

        public PaginatedPlatesViewModel(
            IEnumerable<Plate> plates,
            int currentPage,
            int? previousPage,
            int? nextPage,
            int totalPlates,
            string? sortField,
            SortOrder sortOrder,
            PlateFilter filter
        )
        {
            Plates = plates;
            CurrentPage = currentPage;
            PreviousPage = previousPage;
            NextPage = nextPage;
            TotalPlates = totalPlates;
            SortField = sortField;
            SortOrder = sortOrder;
            Filter = filter;
        }
        
        public PaginatedPlatesViewModel(
            IEnumerable<Plate> plates,
            int currentPage,
            int? previousPage,
            int? nextPage,
            int totalPlates,
            string? sortField,
            SortOrder sortOrder
        )
        {
            Plates = plates;
            CurrentPage = currentPage;
            PreviousPage = previousPage;
            NextPage = nextPage;
            TotalPlates = totalPlates;
            SortField = sortField;
            SortOrder = sortOrder;
        }

        public PaginatedPlatesViewModel(
            IEnumerable<Plate> plates,
            int currentPage,
            int? previousPage,
            int? nextPage,
            int totalPlates
        )
        {
            Plates = plates;
            CurrentPage = currentPage;
            PreviousPage = previousPage;
            NextPage = nextPage;
            TotalPlates = totalPlates;
        }
    }
}