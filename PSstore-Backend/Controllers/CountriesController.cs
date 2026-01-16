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

        public CountriesController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetAllCountries()
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
            return Ok(countryDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> GetCountryById(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
                return NotFound(new { message = "Country not found." });

            var countryDTO = new CountryDTO
            {
                CountryId = country.CountryId,
                CountryCode = country.CountryCode,
                CountryName = country.CountryName,
                Currency = country.Currency,
                RegionId = country.RegionId
            };
            return Ok(countryDTO);
        }
    }

    public class CountryDTO
    {
        public int CountryId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int RegionId { get; set; }
    }
}
