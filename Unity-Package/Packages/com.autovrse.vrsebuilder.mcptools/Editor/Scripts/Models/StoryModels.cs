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
    public sealed class VrseStoryInfoResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsDirty { get; set; }
        public int TotalChapters { get; set; }
        public List<VrseStoryChapterInfo> Chapters { get; set; } = new();
    }

    public sealed class VrseStoryChapterInfo
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MomentCount { get; set; }
        public List<VrseStoryMomentInfo> Moments { get; set; } = new();
    }

    public sealed class VrseStoryMomentInfo
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Defaults { get; set; } = string.Empty;
    }

    public sealed class VrseStoryReadResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool IsSavedToFile { get; set; }
        public int TotalChapters { get; set; }
        public int TotalNodesReturned { get; set; }
        public int MaxNodes { get; set; }
        public List<VrseStoryChapterRead> Chapters { get; set; } = new();
    }

    public sealed class VrseStoryChapterRead
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MomentCount { get; set; }
        public List<VrseStoryMomentRead> Moments { get; set; } = new();
    }

    public sealed class VrseStoryMomentRead
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Defaults { get; set; } = string.Empty;
        public Dictionary<string, object> Sections { get; set; } = new();
    }

    public sealed class VrseStoryNodeData
    {
        public string Name { get; set; } = string.Empty;
        public string Option { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? TargetGameObject { get; set; }
    }

    public sealed class VrseStoryTriggerActionSetData
    {
        public int Index { get; set; }
        public VrseStoryNodeData? Trigger { get; set; }
        public List<VrseStoryNodeData> Actions { get; set; } = new();
    }

    public sealed class VrseStorySaveResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsSavedToFile { get; set; }
    }

    public sealed class VrseStoryValidateResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public int TotalMomentsWithIssues { get; set; }
        public int TotalIssueCount { get; set; }
        public List<VrseStoryValidationIssue> Issues { get; set; } = new();
    }

    public sealed class VrseStoryValidationIssue
    {
        public int ChapterIndex { get; set; }
        public string ChapterName { get; set; } = string.Empty;
        public int MomentIndex { get; set; }
        public string MomentName { get; set; } = string.Empty;
        public int IssueCount { get; set; }
        public Dictionary<string, List<string>> Sections { get; set; } = new();
    }

    public sealed class VrseNodeTemplateListResult
    {
        public bool Success { get; set; }
        public int ActionTemplateCount { get; set; }
        public int TriggerTemplateCount { get; set; }
        public List<VrseNodeTemplateData> ActionTemplates { get; set; } = new();
        public List<VrseNodeTemplateData> TriggerTemplates { get; set; } = new();
        public List<VrseNodeTemplateParameterData> DefaultParameters { get; set; } = new();
    }

    public sealed class VrseNodeTemplateSearchResult
    {
        public bool Success { get; set; }
        public string Query { get; set; } = string.Empty;
        public int ResultCount { get; set; }
        public List<VrseNodeTemplateData> Results { get; set; } = new();
    }

    public sealed class VrseNodeTemplateData
    {
        public string Name { get; set; } = string.Empty;
        public string BackendId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public List<VrseNodeTemplateOptionData> Options { get; set; } = new();
    }

    public sealed class VrseNodeTemplateOptionData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<VrseNodeTemplateParameterData> Parameters { get; set; } = new();
        public List<VrseNodeTemplateNestedParameterData> NestedParameters { get; set; } = new();
    }

    public sealed class VrseNodeTemplateParameterData
    {
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed class VrseNodeTemplateNestedParameterData
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<VrseNodeTemplateParameterData> Parameters { get; set; } = new();
    }
}
