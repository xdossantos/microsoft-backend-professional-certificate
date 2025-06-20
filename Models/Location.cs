using System.ComponentModel.DataAnnotations;

namespace AcmeDealership.Models
{
    public class Location
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        // Calculate distance to another location using Haversine formula
        public double DistanceTo(Location other)
        {
            const double R = 6371; // Earth's radius in km
            
            var lat1Rad = ToRadians(Latitude);
            var lat2Rad = ToRadians(other.Latitude);
            var deltaLatRad = ToRadians(other.Latitude - Latitude);
            var deltaLonRad = ToRadians(other.Longitude - Longitude);

            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }
        
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
