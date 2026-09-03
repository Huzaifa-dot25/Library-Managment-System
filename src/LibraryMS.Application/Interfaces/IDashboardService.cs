using LibraryMS.Application.ViewModels;

namespace LibraryMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsViewModel> GetStatsAsync();
}
