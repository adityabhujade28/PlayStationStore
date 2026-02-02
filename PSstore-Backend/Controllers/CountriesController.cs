using Microsoft.AspNetCore.Mvc;
using PSstore.Interfaces;
using PSstore.DTOs;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountryRepository countryRepository, ILogger<CountriesController> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetAllCountries()
        {
            _logger.LogInformation("Fetching all countries");
            try
            {
                var countries = await _countryRepository.GetAllAsync();
                var countryDTOs = countries.Select(c => new CountryDTO
                {
                    CountryId = c.CountryId,
                    CountryCode = c.CountryCode,
                    CountryName = c.CountryName,
                    Currency = c.Currency,
                    RegionId = c.RegionId
                });
                _logger.LogInformation("Countries retrieved successfully. Count: {Count}", countryDTOs.Count());
                return Ok(countryDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountryById(Guid id)
        {
            _logger.LogInformation("Fetching country by ID: {CountryId}", id);
            try
            {
                var country = await _countryRepository.GetByIdAsync(id);
                if (country == null)
                {
                    _logger.LogWarning("Country not found. ID: {CountryId}", id);
                    return NotFound(new { message = "Country not found." });
                }

                var countryDTO = new CountryDTO
                {
                    CountryId = country.CountryId,
                    CountryCode = country.CountryCode,
                    CountryName = country.CountryName,
                    Currency = country.Currency,
                    RegionId = country.RegionId
                };
                _logger.LogInformation("Country retrieved successfully. ID: {CountryId}, Name: {CountryName}", id, country.CountryName);
                return Ok(countryDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country by ID: {CountryId}", id);
                throw;
            }
        }
    }
}
