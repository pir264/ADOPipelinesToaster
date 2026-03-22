using System;
using System.Collections.Generic;
using System.Linq;

namespace ADOPipelinesToaster.Models;

public class PipelineRun
{
    public int Id { get; set; }
    public int DefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefinitionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public List<PipelineStage> Stages { get; set; } = new();
    public string? WebUrl { get; set; }
    public string? RequestedFor { get; set; }
    public bool HasNewerRunByOther { get; set; }
    public string? NewerRunBy { get; set; }
    public bool AllStagesCompleted => Stages.Count > 0 && Stages.All(s => s.Status == "completed");
}
