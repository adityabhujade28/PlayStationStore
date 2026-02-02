using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories([FromQuery] bool includeDeleted = false)
        {
            _logger.LogInformation("Fetching all categories. IncludeDeleted: {IncludeDeleted}", includeDeleted);
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(includeDeleted);
                _logger.LogInformation("Categories retrieved successfully");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                throw;
            }
        }
    }
}
