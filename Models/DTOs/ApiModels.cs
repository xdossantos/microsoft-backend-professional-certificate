using System.ComponentModel.DataAnnotations;

namespace AcmeGrocer.Models.DTOs
{
    public class FacilityLocationRequest
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }
        
        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }
        
        [Range(1, 50, ErrorMessage = "Count must be between 1 and 50")]
        public int Count { get; set; } = 10;
    }
    
    public class CustomerRecommendationResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double Score { get; set; }
        public double DistanceKm { get; set; }
        public int AcceptedOffers { get; set; }
        public int CanceledOffers { get; set; }
        public double AcceptanceRate { get; set; }
        public int AverageReplyTime { get; set; }
        public bool IsLowDataCustomer { get; set; }
        public string ScoreBreakdown { get; set; } = string.Empty;
    }
    
    public class AnalysisRequest
    {
        public string? AgeGroup { get; set; }
        public double? MinAcceptanceRate { get; set; }
        public double? MaxReplyTime { get; set; }
    }
}
