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

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories([FromQuery] bool includeDeleted = false)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(includeDeleted);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO createCategoryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.CreateCategoryAsync(createCategoryDTO);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDTO>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDTO updateCategoryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDTO);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteCategory(Guid id)
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);
            if (!result)
                return NotFound(new { message = "Category not found." });

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreCategory(Guid id)
        {
            var result = await _categoryService.RestoreCategory(id);
            if (!result)
                return NotFound(new { message = "Category not found." });

            return Ok(new { message = "Category restored successfully." });
        }
    }
}
