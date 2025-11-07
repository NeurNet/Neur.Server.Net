namespace Neur.Server.Net.API;

public class Service {
    public string url {get; set;} = string.Empty;
}

public class ServiceOptions {
    public Service CollegeService {get; set;}
    public Service Frontend {get; set;}
}