using PSstore.DTOs;

namespace PSstore.Interfaces
{
    public interface IAdminService
    {
        Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDTO);
        Task<AdminDTO?> GetAdminByIdAsync(Guid adminId);
        Task<AdminDTO?> GetAdminByEmailAsync(string email);
        Task<DashboardStatsDTO> GetDashboardStatsAsync();
    }
}
