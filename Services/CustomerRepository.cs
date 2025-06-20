using Microsoft.EntityFrameworkCore;
using AcmeGrocer.Data;
using AcmeGrocer.Models;

namespace AcmeGrocer.Services
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(string id);
        Task<List<Customer>> GetCustomersByAgeRangeAsync(int minAge, int maxAge);
        Task<bool> CustomerExistsAsync(string id);
        Task AddCustomerAsync(Customer customer);
        Task AddCustomersAsync(List<Customer> customers);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(string id);
        Task<int> GetTotalCustomersCountAsync();
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerContext _context;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(CustomerContext context, ILogger<CustomerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            try
            {
                return await _context.Customers.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                throw;
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(string id)
        {
            try
            {
                return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
                throw;
            }
        }

        public async Task<List<Customer>> GetCustomersByAgeRangeAsync(int minAge, int maxAge)
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.Age >= minAge && c.Age <= maxAge)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by age range {MinAge}-{MaxAge}", minAge, maxAge);
                throw;
            }
        }

        public async Task<bool> CustomerExistsAsync(string id)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer exists with ID {CustomerId}", id);
                throw;
            }
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added customer {CustomerId}", customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer {CustomerId}", customer.Id);
                throw;
            }
        }

        public async Task AddCustomersAsync(List<Customer> customers)
        {
            try
            {
                _context.Customers.AddRange(customers);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Count} customers", customers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {Count} customers", customers.Count);
                throw;
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            try
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated customer {CustomerId}", customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", customer.Id);
                throw;
            }
        }

        public async Task DeleteCustomerAsync(string id)
        {
            try
            {
                var customer = await GetCustomerByIdAsync(id);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Deleted customer {CustomerId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                throw;
            }
        }

        public async Task<int> GetTotalCustomersCountAsync()
        {
            try
            {
                return await _context.Customers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total customers count");
                throw;
            }
        }
    }
}
