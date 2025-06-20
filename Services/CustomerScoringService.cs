using AcmeDealership.Models;

namespace AcmeDealership.Services
{
    public interface ICustomerScoringService
    {
        Task<List<CustomerScore>> ScoreCustomersAsync(Location facilityLocation, int count = 10);
        CustomerScore CalculateCustomerScore(Customer customer, Location facilityLocation, 
            double minAge, double maxAge, double minDistance, double maxDistance,
            double minAccepted, double maxAccepted, double minCanceled, double maxCanceled,
            double minReplyTime, double maxReplyTime);
    }
    
    public class CustomerScoringService : ICustomerScoringService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerScoringService> _logger;
        private readonly Random _random;

        // Scoring weights as per requirements
        private const double AgeWeight = 0.10;
        private const double DistanceWeight = 0.10;
        private const double AcceptedOffersWeight = 0.30;
        private const double CanceledOffersWeight = 0.30;
        private const double ReplyTimeWeight = 0.20;
        
        // Low data threshold - customers with less than 5 total offers
        private const int LowDataThreshold = 5;

        public CustomerScoringService(ICustomerRepository customerRepository, ILogger<CustomerScoringService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _random = new Random();
        }

        public async Task<List<CustomerScore>> ScoreCustomersAsync(Location facilityLocation, int count = 10)
        {
            try
            {
                _logger.LogInformation("Starting customer scoring for facility at {Latitude}, {Longitude}", 
                    facilityLocation.Latitude, facilityLocation.Longitude);

                var allCustomers = await _customerRepository.GetAllCustomersAsync();
                
                if (!allCustomers.Any())
                {
                    _logger.LogWarning("No customers found in database");
                    return new List<CustomerScore>();
                }

                // Calculate min/max values for normalization
                var minAge = allCustomers.Min(c => c.Age);
                var maxAge = allCustomers.Max(c => c.Age);
                var distances = allCustomers.Select(c => c.Location.DistanceTo(facilityLocation)).ToList();
                var minDistance = distances.Min();
                var maxDistance = distances.Max();
                var minAccepted = allCustomers.Min(c => c.AcceptedOffers);
                var maxAccepted = allCustomers.Max(c => c.AcceptedOffers);
                var minCanceled = allCustomers.Min(c => c.CanceledOffers);
                var maxCanceled = allCustomers.Max(c => c.CanceledOffers);
                var minReplyTime = allCustomers.Min(c => c.AverageReplyTime);
                var maxReplyTime = allCustomers.Max(c => c.AverageReplyTime);

                // Score all customers
                var scoredCustomers = allCustomers.Select(customer => 
                    CalculateCustomerScore(customer, facilityLocation, 
                        minAge, maxAge, minDistance, maxDistance,
                        minAccepted, maxAccepted, minCanceled, maxCanceled,
                        minReplyTime, maxReplyTime)).ToList();

                // Separate low-data customers for special handling
                var lowDataCustomers = scoredCustomers.Where(cs => cs.IsLowDataCustomer).ToList();
                var normalCustomers = scoredCustomers.Where(cs => !cs.IsLowDataCustomer).ToList();

                // Randomly boost some low-data customers to give them a chance
                var lowDataToInclude = Math.Min(lowDataCustomers.Count, count / 3); // Include up to 1/3 as low-data
                var randomLowData = lowDataCustomers.OrderBy(x => _random.Next()).Take(lowDataToInclude).ToList();
                
                // Boost their scores to ensure they appear in top results
                foreach (var customer in randomLowData)
                {
                    customer.Score += _random.NextDouble() * 2; // Add random boost 0-2 points
                }

                // Combine and sort by score
                var finalList = normalCustomers.Concat(randomLowData)
                    .OrderByDescending(cs => cs.Score)
                    .Take(count)
                    .ToList();

                _logger.LogInformation("Successfully scored {TotalCustomers} customers, returning top {Count}", 
                    allCustomers.Count, finalList.Count);

                return finalList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while scoring customers");
                throw;
            }
        }

        public CustomerScore CalculateCustomerScore(Customer customer, Location facilityLocation,
            double minAge, double maxAge, double minDistance, double maxDistance,
            double minAccepted, double maxAccepted, double minCanceled, double maxCanceled,
            double minReplyTime, double maxReplyTime)
        {
            var distance = customer.Location.DistanceTo(facilityLocation);
            var isLowData = customer.TotalOffers < LowDataThreshold;

            // Normalize scores to 0-10 scale
            var ageScore = NormalizeScore(customer.Age, minAge, maxAge, false); // Younger is better
            var distanceScore = NormalizeScore(distance, minDistance, maxDistance, false); // Closer is better
            var acceptedScore = NormalizeScore(customer.AcceptedOffers, minAccepted, maxAccepted, true); // More is better
            var canceledScore = NormalizeScore(customer.CanceledOffers, minCanceled, maxCanceled, false); // Less is better
            var replyTimeScore = NormalizeScore(customer.AverageReplyTime, minReplyTime, maxReplyTime, false); // Faster is better

            // Calculate weighted total score
            var totalScore = (ageScore * AgeWeight) +
                           (distanceScore * DistanceWeight) +
                           (acceptedScore * AcceptedOffersWeight) +
                           (canceledScore * CanceledOffersWeight) +
                           (replyTimeScore * ReplyTimeWeight);

            return new CustomerScore
            {
                Customer = customer,
                Score = totalScore,
                AgeScore = ageScore,
                DistanceScore = distanceScore,
                AcceptedOffersScore = acceptedScore,
                CanceledOffersScore = canceledScore,
                ReplyTimeScore = replyTimeScore,
                IsLowDataCustomer = isLowData
            };
        }

        private static double NormalizeScore(double value, double min, double max, bool higherIsBetter)
        {
            if (max == min) return 5.0; // Default middle score if no variation

            var normalized = (value - min) / (max - min);
            
            // Convert to 1-10 scale
            if (higherIsBetter)
            {
                return 1 + (normalized * 9); // 1-10 where 10 is best
            }
            else
            {
                return 10 - (normalized * 9); // 10-1 where 10 is best (inverse)
            }
        }
    }
}
