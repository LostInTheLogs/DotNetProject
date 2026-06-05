using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IServiceCostReportService
{
    Task<ServiceCostReportDto> GetReportAsync(ServiceCostReportFilterDto filter);
}
