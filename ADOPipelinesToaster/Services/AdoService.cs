using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ADOPipelinesToaster.Models;

namespace ADOPipelinesToaster.Services;

public class AdoService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    public AdoService(AppSettings settings)
    {
        _settings = settings;
        _http = new HttpClient();

        var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + settings.PatToken));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", encoded);
    }

    private string BaseUrl => $"{_settings.OrganizationUrl.TrimEnd('/')}/{_settings.ProjectName}/_apis";
    private string OrgBaseUrl => _settings.OrganizationUrl.TrimEnd('/');

    private string? _currentUserUniqueName;

    private async Task<string?> GetCurrentUserUniqueNameAsync(CancellationToken ct)
    {
        if (_currentUserUniqueName != null) return _currentUserUniqueName;

        var response = await _http.GetAsync($"{OrgBaseUrl}/_apis/connectionData", ct);
        if (!response.IsSuccessStatusCode) return null;

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));
        _currentUserUniqueName = json?["authenticatedUser"]?["properties"]?["Account"]?["$value"]?.GetValue<string>();
        return _currentUserUniqueName;
    }

    public async Task<List<PipelineRun>> GetRecentRunsAsync(CancellationToken ct)
    {
        var uniqueName = await GetCurrentUserUniqueNameAsync(ct);
        var url = $"{BaseUrl}/build/builds?$top=50&api-version=7.1";
        if (!string.IsNullOrEmpty(uniqueName))
            url += $"&requestedFor={Uri.EscapeDataString(uniqueName)}";
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));
        var builds = json?["value"]?.AsArray() ?? new JsonArray();

        // Pick the most recent run per pipeline definition, ordered by most recently started
        var latestPerPipeline = builds
            .Where(b => b != null)
            .GroupBy(b => b!["definition"]?["id"]?.GetValue<int>() ?? 0)
            .Select(g => g.OrderByDescending(b => ParseDate(b?["startTime"])).First())
            .OrderByDescending(b => ParseDate(b?["startTime"]))
            .Take(2);

        var runs = new List<PipelineRun>();
        foreach (var build in latestPerPipeline)
        {
            if (build == null) continue;
            var run = new PipelineRun
            {
                Id = build["id"]?.GetValue<int>() ?? 0,
                Name = build["buildNumber"]?.GetValue<string>() ?? "Unknown",
                Status = build["status"]?.GetValue<string>() ?? string.Empty,
                Result = build["result"]?.GetValue<string>() ?? string.Empty,
                StartTime = ParseDate(build["startTime"]),
                FinishTime = ParseDate(build["finishTime"]),
                WebUrl = build["_links"]?["web"]?["href"]?.GetValue<string>(),
            };

            var definitionName = build["definition"]?["name"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(definitionName))
                run.Name = $"{definitionName} #{run.Name}";

            run.Stages = await GetStagesAsync(run.Id, ct);
            runs.Add(run);
        }

        return runs;
    }

    public async Task<List<PipelineStage>> GetStagesAsync(int buildId, CancellationToken ct)
    {
        var url = $"{BaseUrl}/build/builds/{buildId}/timeline?api-version=7.1";
        var response = await _http.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
            return new List<PipelineStage>();

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));
        var records = json?["records"]?.AsArray() ?? new JsonArray();

        return records
            .Where(r => r?["type"]?.GetValue<string>() == "Stage")
            .OrderBy(r => r?["order"]?.GetValue<int>() ?? 0)
            .Select(r => new PipelineStage
            {
                Name = r?["name"]?.GetValue<string>() ?? "Unknown",
                Status = r?["state"]?.GetValue<string>() ?? string.Empty,
                Result = r?["result"]?.GetValue<string>() ?? string.Empty,
            })
            .ToList();
    }

    private static DateTime? ParseDate(JsonNode? node)
    {
        var s = node?.GetValue<string>();
        return string.IsNullOrEmpty(s) ? null : DateTime.Parse(s);
    }
}
