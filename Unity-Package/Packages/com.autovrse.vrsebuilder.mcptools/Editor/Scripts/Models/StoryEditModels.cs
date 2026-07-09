/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

namespace com.autovrse.vrsebuilder.mcptools.Editor.Models
{
    public sealed class VrseStoryBackupInfo
    {
        public int Index { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string DisplayDate { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class VrseStoryBackupListResult
    {
        public string StoryCreator { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int BackupCount { get; set; }
        public System.Collections.Generic.List<VrseStoryBackupInfo> Backups { get; set; } = new();
    }

    public sealed class VrseStoryBackupResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public VrseStoryBackupInfo? LatestBackup { get; set; }
        public string RestoredBackupPath { get; set; } = string.Empty;
    }

    public sealed class VrseStoryApplyJsonResult
    {
        public bool Success { get; set; }
        public string StoryCreator { get; set; } = string.Empty;
        public int ChapterCount { get; set; }
    }

    public sealed class VrseStoryChapterEditResult
    {
        public bool Success { get; set; }
        public int ChapterIndex { get; set; }
        public string ChapterName { get; set; } = string.Empty;
        public string RemovedChapterName { get; set; } = string.Empty;
        public int RemainingCount { get; set; }
    }

    public sealed class VrseStoryMomentEditResult
    {
        public bool Success { get; set; }
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public string MomentName { get; set; } = string.Empty;
        public string RemovedMomentName { get; set; } = string.Empty;
        public int RemainingCount { get; set; }
    }

    public sealed class VrseStoryTriggerSetEditResult
    {
        public bool Success { get; set; }
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public string Section { get; set; } = string.Empty;
        public int TriggerSetIndex { get; set; }
        public VrseStoryNodeData? Trigger { get; set; }
        public int ActionCount { get; set; }
    }

    public sealed class VrseStoryNodeEditResult
    {
        public bool Success { get; set; }
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public string Section { get; set; } = string.Empty;
        public int TriggerSetIndex { get; set; }
        public int NodeIndex { get; set; }
        public string NodeKind { get; set; } = string.Empty;
        public VrseStoryNodeData? Node { get; set; }
        public VrseStoryNodeData? RemovedNode { get; set; }
        public int RemainingCount { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public int SourceNodeIndex { get; set; }
        public int NewNodeIndex { get; set; }
    }

    public sealed class VrseStoryApplyActionTarget
    {
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public string? Section { get; set; }
        public int TriggerSetIndex { get; set; } = -1;
    }

    public sealed class VrseStoryApplyActionTargetResult
    {
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public string Section { get; set; } = string.Empty;
        public int TriggerSetIndex { get; set; }
        public bool Success { get; set; }
        public int NewNodeIndex { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public sealed class VrseStoryApplyActionMultipleResult
    {
        public bool Success { get; set; }
        public int SourceChapterIndex { get; set; }
        public int SourceMomentIndex { get; set; }
        public string SourceSection { get; set; } = string.Empty;
        public int SourceTriggerSetIndex { get; set; }
        public int SourceNodeIndex { get; set; }
        public VrseStoryNodeData? SourceNode { get; set; }
        public System.Collections.Generic.List<VrseStoryApplyActionTargetResult> Targets { get; set; } = new();
    }

    public sealed class VrseMomentWeightageResult
    {
        public bool Success { get; set; }
        public int ChapterIndex { get; set; }
        public int MomentIndex { get; set; }
        public float Weightage { get; set; }
        public float WrongReduction { get; set; }
    }
}
