using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;

namespace ClinicManager.BackgroundServices;

public class UpcomingVisitsReportBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<UpcomingVisitsReportBackgroundService> logger,
    IConfiguration configuration, IWebHostEnvironment environment)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool isDevelopment = environment.IsDevelopment();
        logger.LogInformation("Upcoming Visits Automated Report Worker Engine initialized.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TimeSpan delayDuration;
                DateTime targetDate;

                if (isDevelopment)
                {
                    delayDuration = TimeSpan.FromMinutes(2);
                    targetDate = DateTime.Today;
                }
                else
                {
                    var now = DateTime.Now;
                    var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);

                    if (now > nextRunTime)
                    {
                        nextRunTime = nextRunTime.AddDays(1);
                    }

                    delayDuration = nextRunTime - now;
                    targetDate = DateTime.Today.AddDays(1);
                }

                logger.LogInformation("Next report pipeline cycle scheduled execution in: {TimeSpan}", delayDuration);

                await Task.Delay(delayDuration, stoppingToken);

                logger.LogInformation("Making report for for date: {TargetDate:yyyy-MM-dd}...", targetDate);

                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();

                    var tomorrowDate = DateTime.Today.AddDays(1);
                    var startOfTomorrow = tomorrowDate.Date;
                    var endOfTomorrow = startOfTomorrow.AddDays(1).AddTicks(-1);

                    var tomorrowVisits = await context.Visits
                        .Include(v => v.Patient)
                        .Include(v => v.Doctor)
                        .AsNoTracking()
                        .Where(v => v.ScheduledDate >= startOfTomorrow && v.ScheduledDate <= endOfTomorrow)
                        .Where(v => v.Status == VisitStatus.Scheduled || v.Status == VisitStatus.InProgress)
                        .OrderBy(v => v.ScheduledDate)
                        .Select(v => new VisitDetailsDto
                        {
                            Id = v.Id,
                            ScheduledDate = v.ScheduledDate,
                            Status = v.Status,
                            Reason = v.Reason,
                            PatientFullName = v.Patient != null ? $"{v.Patient.LastName}, {v.Patient.FirstName}" : "Unknown Patient",
                            DoctorFullName = v.Doctor != null ? $"{v.Doctor.FirstName} {v.Doctor.LastName}" : "Unassigned Staff"
                        })
                        .ToListAsync(stoppingToken);

                    byte[] pdfAttachmentBytes = pdfService.GenerateUpcomingVisitsReportPdf(tomorrowDate, tomorrowVisits);

                    await SendManagerEmailAsync(pdfAttachmentBytes, tomorrowDate);
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Background schedule delivery worker loop interrupted via host commands.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "A functional exception occurred inside the background reporting pipeline daemon.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task SendManagerEmailAsync(byte[] pdfBytes, DateTime targetDate)
    {
        var managerEmail = configuration["ReportingSettings:ManagerEmailAddress"] ?? "manager@clinic.com";
        var smtpHost = configuration["Smtp:Host"] ?? "127.0.0.1";
        var smtpPort = int.Parse(configuration["Smtp:Port"] ?? "1025");

        using var message = new MailMessage();
        message.From = new MailAddress("automated-reporting@clinic.com", "The Abysmal Medical Center Automated Engine");
        message.To.Add(new MailAddress(managerEmail));
        message.Subject = $"[OPERATIONS REPORT] Scheduled Clinic Visits Briefing - {targetDate:yyyy-MM-dd}";
        message.Body = $"Good morning,\n\nPlease locate the attached daily schedule briefing outlining all upcoming clinical procedures and visits configured for tomorrow: {targetDate:dddd, MMMM dd, yyyy}.\n\nBest Regards,\nOperations Automation Engine";

        using var stream = new MemoryStream(pdfBytes);
        var reportAttachment = new Attachment(stream, $"UpcomingVisitsSchedule_{targetDate:yyyyMMdd}.pdf", "application/pdf");
        message.Attachments.Add(reportAttachment);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);
        smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;

        logger.LogInformation("Dispatching schedule report payload to manager SMTP host target at {Host}:{Port}", smtpHost, smtpPort);
        await smtpClient.SendMailAsync(message);
        logger.LogInformation("Morning operational schedule update delivered successfully.");
    }
}
