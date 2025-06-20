using AcmeDealership.Models;

namespace AcmeDealership.Services
{
    public interface IBinarySerializationService
    {
        Task SerializeCustomerAsync(Customer customer, string filePath);
        Task<Customer?> DeserializeCustomerAsync(string filePath);
        Task SerializeCustomersAsync(List<Customer> customers, string filePath);
        Task<List<Customer>> DeserializeCustomersAsync(string filePath);
    }

    public class BinarySerializationService : IBinarySerializationService
    {
        private readonly ILogger<BinarySerializationService> _logger;

        public BinarySerializationService(ILogger<BinarySerializationService> logger)
        {
            _logger = logger;
        }

        public async Task SerializeCustomerAsync(Customer customer, string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Create);
                using var writer = new BinaryWriter(fs);
                
                // Write customer data
                writer.Write(customer.Id);
                writer.Write(customer.Name);
                writer.Write(customer.Age);
                writer.Write(customer.Location.Latitude);
                writer.Write(customer.Location.Longitude);
                writer.Write(customer.AcceptedOffers);
                writer.Write(customer.CanceledOffers);
                writer.Write(customer.AverageReplyTime);
                
                await fs.FlushAsync();
                _logger.LogInformation("Successfully serialized customer {CustomerId} to {FilePath}", customer.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing customer {CustomerId} to {FilePath}", customer.Id, filePath);
                throw;
            }
        }

        public Task<Customer?> DeserializeCustomerAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return Task.FromResult<Customer?>(null);
                }

                using var fs = new FileStream(filePath, FileMode.Open);
                using var reader = new BinaryReader(fs);
                
                var customer = new Customer
                {
                    Id = reader.ReadString(),
                    Name = reader.ReadString(),
                    Age = reader.ReadInt32(),
                    Location = new Location
                    {
                        Latitude = reader.ReadDouble(),
                        Longitude = reader.ReadDouble()
                    },
                    AcceptedOffers = reader.ReadInt32(),
                    CanceledOffers = reader.ReadInt32(),
                    AverageReplyTime = reader.ReadInt32()
                };

                _logger.LogInformation("Successfully deserialized customer {CustomerId} from {FilePath}", customer.Id, filePath);
                return Task.FromResult<Customer?>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing customer from {FilePath}", filePath);
                throw;
            }
        }

        public async Task SerializeCustomersAsync(List<Customer> customers, string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Create);
                using var writer = new BinaryWriter(fs);
                
                // Write number of customers first
                writer.Write(customers.Count);
                
                // Write each customer
                foreach (var customer in customers)
                {
                    writer.Write(customer.Id);
                    writer.Write(customer.Name);
                    writer.Write(customer.Age);
                    writer.Write(customer.Location.Latitude);
                    writer.Write(customer.Location.Longitude);
                    writer.Write(customer.AcceptedOffers);
                    writer.Write(customer.CanceledOffers);
                    writer.Write(customer.AverageReplyTime);
                }
                
                await fs.FlushAsync();
                _logger.LogInformation("Successfully serialized {Count} customers to {FilePath}", customers.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing {Count} customers to {FilePath}", customers.Count, filePath);
                throw;
            }
        }

        public Task<List<Customer>> DeserializeCustomersAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return Task.FromResult(new List<Customer>());
                }

                using var fs = new FileStream(filePath, FileMode.Open);
                using var reader = new BinaryReader(fs);
                
                var customers = new List<Customer>();
                var count = reader.ReadInt32();
                
                for (int i = 0; i < count; i++)
                {
                    var customer = new Customer
                    {
                        Id = reader.ReadString(),
                        Name = reader.ReadString(),
                        Age = reader.ReadInt32(),
                        Location = new Location
                        {
                            Latitude = reader.ReadDouble(),
                            Longitude = reader.ReadDouble()
                        },
                        AcceptedOffers = reader.ReadInt32(),
                        CanceledOffers = reader.ReadInt32(),
                        AverageReplyTime = reader.ReadInt32()
                    };
                    
                    customers.Add(customer);
                }

                _logger.LogInformation("Successfully deserialized {Count} customers from {FilePath}", customers.Count, filePath);
                return Task.FromResult(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing customers from {FilePath}", filePath);
                throw;
            }
        }
    }
}
