namespace ADOPipelinesToaster.Models;

public class AppSettings
{
    public string OrganizationUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string PatToken { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; } = 60;
}
