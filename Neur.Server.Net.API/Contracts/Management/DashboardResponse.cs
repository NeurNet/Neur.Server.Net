namespace Neur.Server.Net.API.Contracts.Management;

public record DashboardResponse(int requests_count, int users_count, int? models_count);
