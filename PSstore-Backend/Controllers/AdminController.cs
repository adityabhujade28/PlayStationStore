using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO loginDTO)
        {
            _logger.LogInformation("Admin login attempt for email: {Email}", loginDTO.UserEmail);
            try
            {
                var response = await _adminService.LoginAsync(loginDTO);
                if (response == null)
                {
                    _logger.LogWarning("Admin login failed for email: {Email}. Invalid credentials.", loginDTO.UserEmail);
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                _logger.LogInformation("Admin login successful for email: {Email}", loginDTO.UserEmail);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login for email: {Email}", loginDTO.UserEmail);
                throw;
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDTO>> GetStats()
        {
            _logger.LogInformation("Fetching dashboard statistics");
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                _logger.LogInformation("Dashboard statistics retrieved successfully");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                throw;
            }
        }
    }
}
