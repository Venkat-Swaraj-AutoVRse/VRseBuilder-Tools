/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.Collections.Generic;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Models
{
    public sealed class VrseBuildWindowResult
    {
        public bool Success { get; set; }
        public string Window { get; set; } = string.Empty;
    }

    public sealed class VrseModuleIncludeResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleId { get; set; } = string.Empty;
        public bool IncludeInBuild { get; set; }
    }

    public sealed class VrseBuildStartResult
    {
        public bool Success { get; set; }
        public string Result { get; set; } = string.Empty;
        public int TotalErrors { get; set; }
        public int TotalWarnings { get; set; }
        public string OutputPath { get; set; } = string.Empty;
        public List<string> Scenes { get; set; } = new();
    }

    public sealed class VrseBuildStatusResult
    {
        public string Status { get; set; } = "ready";
        public string Message { get; set; } = string.Empty;
    }

    public sealed class VrseEvaluationCreateResult
    {
        public bool Success { get; set; }
        public string EvaluationScene { get; set; } = string.Empty;
        public string EvaluationStory { get; set; } = string.Empty;
        public string StoryDefaults { get; set; } = string.Empty;
    }
}
