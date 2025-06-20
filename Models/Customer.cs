using System.ComponentModel.DataAnnotations;

namespace AcmeGrocer.Models
{
    public class Customer
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Range(18, 120)]
        public int Age { get; set; }
        
        [Required]
        public Location Location { get; set; } = new Location();
        
        [Range(0, int.MaxValue)]
        public int AcceptedOffers { get; set; }
        
        [Range(0, int.MaxValue)]
        public int CanceledOffers { get; set; }
        
        [Range(0, int.MaxValue)]
        public int AverageReplyTime { get; set; }
        
        // Computed property for total offers
        public int TotalOffers => AcceptedOffers + CanceledOffers;
        
        // Computed property for acceptance rate
        public double AcceptanceRate => TotalOffers > 0 ? (double)AcceptedOffers / TotalOffers : 0;
    }
}
