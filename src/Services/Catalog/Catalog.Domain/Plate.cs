using System.ComponentModel.DataAnnotations.Schema;

namespace Catalog.Domain
{
    public class Plate
    {
        public Guid Id { get; set; }

        public string? Registration { get; set; }

        [Column(TypeName="decimal(18,4)")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName="decimal(18,4)")]
        public decimal SalePrice { get; set; }

        public string? Letters { get; set; }

        public int Numbers { get; set; }
    }
}