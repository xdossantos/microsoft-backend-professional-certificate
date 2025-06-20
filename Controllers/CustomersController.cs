using Microsoft.AspNetCore.Mvc;
using AcmeGrocer.Models;
using AcmeGrocer.Models.DTOs;
using AcmeGrocer.Services;

namespace AcmeGrocer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerScoringService _scoringService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBinarySerializationService _binaryService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerScoringService scoringService,
            ICustomerRepository customerRepository,
            IBinarySerializationService binaryService,
            ILogger<CustomersController> logger)
        {
            _scoringService = scoringService;
            _customerRepository = customerRepository;
            _binaryService = binaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get prioritized customer recommendations for a facility location
        /// </summary>
        [HttpPost("recommendations")]
        public async Task<ActionResult<List<CustomerRecommendationResponse>>> GetCustomerRecommendations(
            [FromBody] FacilityLocationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var facilityLocation = new Location
                {
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                var scoredCustomers = await _scoringService.ScoreCustomersAsync(facilityLocation, request.Count);

                var response = scoredCustomers.Select(sc => new CustomerRecommendationResponse
                {
                    Id = sc.Customer.Id,
                    Name = sc.Customer.Name,
                    Age = sc.Customer.Age,
                    Score = Math.Round(sc.Score, 2),
                    DistanceKm = Math.Round(sc.Customer.Location.DistanceTo(facilityLocation), 2),
                    AcceptedOffers = sc.Customer.AcceptedOffers,
                    CanceledOffers = sc.Customer.CanceledOffers,
                    AcceptanceRate = Math.Round(sc.Customer.AcceptanceRate, 2),
                    AverageReplyTime = sc.Customer.AverageReplyTime,
                    IsLowDataCustomer = sc.IsLowDataCustomer,
                    ScoreBreakdown = sc.ScoreBreakdown
                }).ToList();

                _logger.LogInformation("Returned {Count} customer recommendations for facility at {Latitude}, {Longitude}", 
                    response.Count, request.Latitude, request.Longitude);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer recommendations");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get all customers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Customer>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerRepository.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(string id)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerByIdAsync(id);
                
                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while retrieving the customer.");
            }
        }

        /// <summary>
        /// Get customers by age range
        /// </summary>
        [HttpGet("age-range")]
        public async Task<ActionResult<List<Customer>>> GetCustomersByAgeRange(
            [FromQuery] int minAge = 18, 
            [FromQuery] int maxAge = 120)
        {
            try
            {
                if (minAge < 0 || maxAge < 0 || minAge > maxAge)
                {
                    return BadRequest("Invalid age range provided.");
                }

                var customers = await _customerRepository.GetCustomersByAgeRangeAsync(minAge, maxAge);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by age range {MinAge}-{MaxAge}", minAge, maxAge);
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _customerRepository.CustomerExistsAsync(customer.Id))
                {
                    return Conflict($"Customer with ID {customer.Id} already exists.");
                }

                await _customerRepository.AddCustomerAsync(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer {CustomerId}", customer.Id);
                return StatusCode(500, "An error occurred while creating the customer.");
            }
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCustomer(string id, [FromBody] Customer customer)
        {
            try
            {
                if (id != customer.Id)
                {
                    return BadRequest("ID in URL does not match ID in request body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await _customerRepository.CustomerExistsAsync(id))
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                await _customerRepository.UpdateCustomerAsync(customer);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while updating the customer.");
            }
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(string id)
        {
            try
            {
                if (!await _customerRepository.CustomerExistsAsync(id))
                {
                    return NotFound($"Customer with ID {id} not found.");
                }

                await _customerRepository.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while deleting the customer.");
            }
        }

        /// <summary>
        /// Export customer data using binary serialization
        /// </summary>
        [HttpPost("export")]
        public async Task<ActionResult> ExportCustomers([FromQuery] string? customerId = null)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                
                if (!string.IsNullOrEmpty(customerId))
                {
                    var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                    if (customer == null)
                    {
                        return NotFound($"Customer with ID {customerId} not found.");
                    }

                    var fileName = $"customer_{customerId}_{timestamp}.dat";
                    var filePath = Path.Combine(Path.GetTempPath(), fileName);
                    
                    await _binaryService.SerializeCustomerAsync(customer, filePath);
                    
                    return Ok(new { Message = "Customer exported successfully", FilePath = filePath });
                }
                else
                {
                    var customers = await _customerRepository.GetAllCustomersAsync();
                    var fileName = $"customers_all_{timestamp}.dat";
                    var filePath = Path.Combine(Path.GetTempPath(), fileName);
                    
                    await _binaryService.SerializeCustomersAsync(customers, filePath);
                    
                    return Ok(new { Message = $"All {customers.Count} customers exported successfully", FilePath = filePath });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting customers");
                return StatusCode(500, "An error occurred while exporting customers.");
            }
        }

        /// <summary>
        /// Get customer statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetCustomerStatistics()
        {
            try
            {
                var customers = await _customerRepository.GetAllCustomersAsync();
                
                if (!customers.Any())
                {
                    return Ok(new { Message = "No customers found" });
                }

                var stats = new
                {
                    TotalCustomers = customers.Count,
                    AverageAge = Math.Round(customers.Average(c => c.Age), 1),
                    AgeRanges = new
                    {
                        Young_18_30 = customers.Count(c => c.Age >= 18 && c.Age <= 30),
                        MiddleAge_31_50 = customers.Count(c => c.Age >= 31 && c.Age <= 50),
                        Senior_51_Plus = customers.Count(c => c.Age >= 51)
                    },
                    OfferStatistics = new
                    {
                        TotalAcceptedOffers = customers.Sum(c => c.AcceptedOffers),
                        TotalCanceledOffers = customers.Sum(c => c.CanceledOffers),
                        AverageAcceptanceRate = Math.Round(customers.Average(c => c.AcceptanceRate), 2),
                        AverageReplyTime = Math.Round(customers.Average(c => c.AverageReplyTime), 0)
                    },
                    TopPerformers = customers.OrderByDescending(c => c.AcceptanceRate)
                        .Take(5)
                        .Select(c => new { c.Id, c.Name, AcceptanceRate = Math.Round(c.AcceptanceRate, 2) })
                        .ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics");
                return StatusCode(500, "An error occurred while retrieving statistics.");
            }
        }
    }
}
