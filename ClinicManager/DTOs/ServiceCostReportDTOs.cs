namespace ClinicManager.DTOs;

public class ServiceCostReportFilterDto
{
    public int? PatientId { get; set; }
    public string? DoctorId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class ServiceCostReportLineDto
{
    public int VisitId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string DoctorName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public int ProcedureCount { get; init; }
    public int MedicationCount { get; init; }
    public decimal TotalCost { get; init; }
}

public class ServiceCostReportDto
{
    public List<ServiceCostReportLineDto> Lines { get; init; } = new();
    public decimal GrandTotal { get; set; }
    public int TotalVisits { get; set; }
    public string FilterDescription { get; set; } = string.Empty;
}
