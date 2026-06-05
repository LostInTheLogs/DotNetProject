using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Contracts.Stats;

namespace ClinicManager.PerformanceTests;

public class PerfTests
{
    public static void Run()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5000");

        var visits_scenario = Scenario.Create("clinic_active_visits_load_scenario", async context =>
        {
            var request = Http.CreateRequest("GET", "/api/visits/active")
                              .WithHeader("Accept", "application/json");

            var response = await Http.Send(httpClient, request);

            return response.IsError
                ? Response.Fail(statusCode: response.StatusCode)
                : Response.Ok(statusCode: response.StatusCode);
        })
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(30))
        );

        var testQueries = new[] { "Kowalski", "Anna", "920", "Jan", "   ", "Nowak", "", "Smith" };
        var random = new Random();

        var patient_scenario = Scenario.Create("patient_search_load_scenario", async context =>
            {
                try
                {
                    string selectedQuery = testQueries[random.Next(testQueries.Length)];

                    var request = Http.CreateRequest("GET", $"/api/patients/search?query={Uri.EscapeDataString(selectedQuery)}")
                        .WithHeader("Accept", "application/json");

                    var nBomberResponse = await Http.Send(httpClient, request);

                    var httpResponse = nBomberResponse.Payload;
                    System.Net.HttpStatusCode statusCode = httpResponse.Value.StatusCode;

                    if (statusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Response.Ok(statusCode: "200");
                    }

                    if (statusCode == System.Net.HttpStatusCode.BadRequest && string.IsNullOrWhiteSpace(selectedQuery))
                    {
                        // Expected validation path pass
                        return Response.Ok(statusCode: "400_ValidationPass");
                    }

                    return Response.Fail(
                        message: $"Unexpected HTTP Status: {statusCode}",
                        statusCode: ((int)statusCode).ToString()
                    );
                }
                catch (Exception ex)
                {
                    return Response.Fail(message: ex.Message);
                }
            })
            .WithLoadSimulations(
                Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(30))
            );

        NBomberRunner
           .RegisterScenarios(visits_scenario, patient_scenario)
           .WithReportFileName("nbomber-report")
           .Run();
    }
}
