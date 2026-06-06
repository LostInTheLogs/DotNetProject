using System.Reflection;
using ClinicManager.Controllers;
using ClinicManager.Controllers.Api;
using ClinicManager.DTOs;
using ClinicManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Tests;

public class RoutingTests
{
    // ==========================================
    // Controller-level authorization
    // ==========================================

    [Fact]
    public void AdminController_RequiresAdminRole()
    {
        var attr = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin", attr.Roles);
    }

    [Fact]
    public void PatientController_RequiresAuthenticatedStaffRoles()
    {
        var attr = typeof(PatientController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin,Doctor,Receptionist", attr.Roles);
    }

    [Fact]
    public void VisitController_RequiresAuthenticatedStaffRoles()
    {
        var attr = typeof(VisitController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin,Doctor,Receptionist", attr.Roles);
    }

    [Fact]
    public void MedicationController_RequiresAdminOrReceptionist()
    {
        var attr = typeof(MedicationController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin,Receptionist", attr.Roles);
    }

    [Fact]
    public void MedicalProcedureController_RequiresAdminOrReceptionist()
    {
        var attr = typeof(MedicalProcedureController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin,Receptionist", attr.Roles);
    }

    [Fact]
    public void ReportsController_RequiresAdmin()
    {
        var attr = typeof(ReportsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Admin", attr.Roles);
    }

    [Fact]
    public void HomeController_RequiresAuthentication()
    {
        var attr = typeof(HomeController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Null(attr.Roles);
    }

    // ==========================================
    // Account controller: AllowAnonymous on public endpoints
    // ==========================================

    [Fact]
    public void AccountController_LoginGet_AllowsAnonymous()
    {
        var method = typeof(AccountController).GetMethod("Login", [typeof(string)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void AccountController_LoginPost_AllowsAnonymous()
    {
        var method = typeof(AccountController).GetMethod("Login", [typeof(LoginDto), typeof(string)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void AccountController_RegisterGet_AllowsAnonymous()
    {
        var method = typeof(AccountController).GetMethod("Register", Type.EmptyTypes);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void AccountController_RegisterPost_AllowsAnonymous()
    {
        var method = typeof(AccountController).GetMethod("Register", [typeof(RegisterDto)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    // ==========================================
    // HTTP method attributes
    // ==========================================

    [Fact]
    public void PatientController_CreateGet_IsHttpGet()
    {
        var method = typeof(PatientController).GetMethod("Create", Type.EmptyTypes);
        Assert.NotNull(method);
        Assert.Null(method.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void PatientController_CreatePost_IsHttpPost()
    {
        var method = typeof(PatientController).GetMethod("Create", [typeof(CreatePatientRequestDto)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void PatientController_DeletePost_IsHttpPost()
    {
        var method = typeof(PatientController).GetMethod("Delete", [typeof(int)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void VisitController_CalendarGet_IsHttpGet()
    {
        var method = typeof(VisitController).GetMethod("Calendar", [typeof(string), typeof(DateTime?)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public void VisitController_UpdateStatusPost_IsHttpPost()
    {
        var method = typeof(VisitController).GetMethod("UpdateStatus", [typeof(int), typeof(VisitStatus), typeof(string)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void VisitController_DownloadSummaryGet_IsHttpGet()
    {
        var method = typeof(VisitController).GetMethod("DownloadSummary", [typeof(int)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public void MedicationController_IndexGet_IsHttpGet()
    {
        var method = typeof(MedicationController).GetMethod("Index", Type.EmptyTypes);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public void MedicationController_ToggleStatusPost_IsHttpPost()
    {
        var method = typeof(MedicationController).GetMethod("ToggleStatus", [typeof(int)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
    }

    [Fact]
    public void ReportsController_ExportPdfGet_IsHttpGet()
    {
        var method = typeof(ReportsController).GetMethod("ExportPdf", [typeof(ServiceCostReportFilterDto)]);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public void AccountController_LogoutPost_IsHttpPost()
    {
        var method = typeof(AccountController).GetMethod("Logout", Type.EmptyTypes);
        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
    }

    // ==========================================
    // API controller routes
    // ==========================================

    [Fact]
    public void VisitsApiController_HasApiRoute()
    {
        var routeAttr = typeof(VisitsApiController).GetCustomAttribute<RouteAttribute>();
        Assert.NotNull(routeAttr);
        Assert.Equal("api", routeAttr.Template);
    }

    [Fact]
    public void VisitsApiController_IsApiController()
    {
        var attr = typeof(VisitsApiController).GetCustomAttribute<ApiControllerAttribute>();
        Assert.NotNull(attr);
    }

    [Fact]
    public void VisitsApiController_GetActiveVisits_HasCorrectRoute()
    {
        var method = typeof(VisitsApiController).GetMethod("GetActiveVisits", [typeof(int)]);
        Assert.NotNull(method);
        var attr = method.GetCustomAttribute<HttpGetAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("visits/active", attr.Template);
    }

    [Fact]
    public void VisitsApiController_SearchPatients_HasCorrectRoute()
    {
        var method = typeof(VisitsApiController).GetMethod("SearchPatients", [typeof(string)]);
        Assert.NotNull(method);
        var attr = method.GetCustomAttribute<HttpGetAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("patients/search", attr.Template);
    }

    // ==========================================
    // Action signatures ensure controllers compile correctly
    // ==========================================

    [Fact]
    public void AdminController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(AdminController).GetMethod("Users", Type.EmptyTypes));
        Assert.NotNull(typeof(AdminController).GetMethod("CreateStaff", Type.EmptyTypes));
        Assert.NotNull(typeof(AdminController).GetMethod("CreateStaff", [typeof(CreateStaffDto)]));
        Assert.NotNull(typeof(AdminController).GetMethod("ManageRoles", [typeof(string)]));
        Assert.NotNull(typeof(AdminController).GetMethod("ManageRoles", [typeof(ManageRolesDto)]));
        Assert.NotNull(typeof(AdminController).GetMethod("DeleteUser", [typeof(string)]));
    }

    [Fact]
    public void PatientController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(PatientController).GetMethod("Index", [typeof(string)]));
        Assert.NotNull(typeof(PatientController).GetMethod("Details", [typeof(int)]));
        Assert.NotNull(typeof(PatientController).GetMethod("Create", Type.EmptyTypes));
        Assert.NotNull(typeof(PatientController).GetMethod("Create", [typeof(CreatePatientRequestDto)]));
        Assert.NotNull(typeof(PatientController).GetMethod("Edit", [typeof(int)]));
        Assert.NotNull(typeof(PatientController).GetMethod("Edit", [typeof(int), typeof(UpdatePatientRequestDto)]));
        Assert.NotNull(typeof(PatientController).GetMethod("Delete", [typeof(int)]));
        Assert.NotNull(typeof(PatientController).GetMethod("MedicalRecords", [typeof(int)]));
    }

    [Fact]
    public void VisitController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(VisitController).GetMethod("Calendar", [typeof(string), typeof(DateTime?)]));
        Assert.NotNull(typeof(VisitController).GetMethod("Manage", [typeof(int)]));
        Assert.NotNull(typeof(VisitController).GetMethod("Book", [typeof(int), typeof(string), typeof(DateTime), typeof(string)]));
        Assert.NotNull(typeof(VisitController).GetMethod("UpdateStatus", [typeof(int), typeof(VisitStatus), typeof(string)]));
        Assert.NotNull(typeof(VisitController).GetMethod("AddProcedure", [typeof(int), typeof(LogProcedurePerformedDto)]));
        Assert.NotNull(typeof(VisitController).GetMethod("AddPrescription", [typeof(int), typeof(AddPrescribedMedicationDto)]));
        Assert.NotNull(typeof(VisitController).GetMethod("AddNote", [typeof(CreateClinicalNoteDto)]));
        Assert.NotNull(typeof(VisitController).GetMethod("DownloadSummary", [typeof(int)]));
        Assert.NotNull(typeof(VisitController).GetMethod("DownloadPrescription", [typeof(int)]));
    }

    [Fact]
    public void MedicationController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(MedicationController).GetMethod("Index", Type.EmptyTypes));
        Assert.NotNull(typeof(MedicationController).GetMethod("Create", Type.EmptyTypes));
        Assert.NotNull(typeof(MedicationController).GetMethod("Create", [typeof(Medication)]));
        Assert.NotNull(typeof(MedicationController).GetMethod("Edit", [typeof(int)]));
        Assert.NotNull(typeof(MedicationController).GetMethod("Edit", [typeof(Medication)]));
        Assert.NotNull(typeof(MedicationController).GetMethod("ToggleStatus", [typeof(int)]));
    }

    [Fact]
    public void ReportsController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(ReportsController).GetMethod("Index", [typeof(ServiceCostReportFilterDto)]));
        Assert.NotNull(typeof(ReportsController).GetMethod("ExportPdf", [typeof(ServiceCostReportFilterDto)]));
    }

    [Fact]
    public void AccountController_HasAllRequiredActions()
    {
        Assert.NotNull(typeof(AccountController).GetMethod("Login", [typeof(string)]));
        Assert.NotNull(typeof(AccountController).GetMethod("Login", [typeof(LoginDto), typeof(string)]));
        Assert.NotNull(typeof(AccountController).GetMethod("Register", Type.EmptyTypes));
        Assert.NotNull(typeof(AccountController).GetMethod("Register", [typeof(RegisterDto)]));
        Assert.NotNull(typeof(AccountController).GetMethod("Logout", Type.EmptyTypes));
        Assert.NotNull(typeof(AccountController).GetMethod("AccessDenied", Type.EmptyTypes));
    }
}
