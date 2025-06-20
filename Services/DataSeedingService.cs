using System.Text.Json;
using AcmeDealership.Models;
using AcmeDealership.Services;

namespace AcmeDealership.Data
{
    public interface IDataSeedingService
    {
        Task SeedCustomersFromJsonAsync();
        Task<bool> IsDatabaseSeededAsync();
    }

    public class DataSeedingService : IDataSeedingService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<DataSeedingService> _logger;
        private readonly string _jsonFilePath;

        public DataSeedingService(ICustomerRepository customerRepository, ILogger<DataSeedingService> logger, IWebHostEnvironment env)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _jsonFilePath = Path.Combine(env.ContentRootPath, "sample-data", "customers.json");
        }

        public async Task<bool> IsDatabaseSeededAsync()
        {
            try
            {
                var count = await _customerRepository.GetTotalCustomersCountAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if database is seeded");
                return false;
            }
        }

        public async Task SeedCustomersFromJsonAsync()
        {
            try
            {
                if (await IsDatabaseSeededAsync())
                {
                    _logger.LogInformation("Database already seeded, skipping");
                    return;
                }

                if (!File.Exists(_jsonFilePath))
                {
                    _logger.LogWarning("Customer JSON file not found at {FilePath}", _jsonFilePath);
                    return;
                }

                _logger.LogInformation("Starting to seed customers from {FilePath}", _jsonFilePath);

                var jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var customerData = JsonSerializer.Deserialize<List<CustomerJsonModel>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (customerData == null || !customerData.Any())
                {
                    _logger.LogWarning("No customer data found in JSON file");
                    return;
                }

                var customers = customerData.Select(MapToCustomer).ToList();
                
                await _customerRepository.AddCustomersAsync(customers);
                
                _logger.LogInformation("Successfully seeded {Count} customers", customers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding customers from JSON");
                throw;
            }
        }

        private static Customer MapToCustomer(CustomerJsonModel jsonModel)
        {
            return new Customer
            {
                Id = jsonModel.Id,
                Name = jsonModel.Name,
                Age = jsonModel.Age,
                Location = new Location
                {
                    Latitude = double.Parse(jsonModel.Location.Latitude),
                    Longitude = double.Parse(jsonModel.Location.Longitude)
                },
                AcceptedOffers = jsonModel.AcceptedOffers,
                CanceledOffers = jsonModel.CanceledOffers,
                AverageReplyTime = jsonModel.AverageReplyTime
            };
        }
    }

    // Helper class for JSON deserialization
    public class CustomerJsonModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public LocationJsonModel Location { get; set; } = new LocationJsonModel();
        public int Age { get; set; }
        public int AcceptedOffers { get; set; }
        public int CanceledOffers { get; set; }
        public int AverageReplyTime { get; set; }
    }

    public class LocationJsonModel
    {
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
    }
}
