using System;
using System.Collections.Generic;

namespace ADOPipelinesToaster.Models;

public class PipelineRun
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public List<PipelineStage> Stages { get; set; } = new();
}
