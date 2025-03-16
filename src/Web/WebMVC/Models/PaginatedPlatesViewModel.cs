namespace WebMVC.Models
{
    public class PaginatedPlatesViewModel
    {
        public IEnumerable<Plate> Plates {get;}
        public int CurrentPage {get;}
        public int? NextPage {get;}
        public int? PreviousPage {get;}
        public int TotalPlates { get; }

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