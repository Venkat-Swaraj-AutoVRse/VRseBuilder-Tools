/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

namespace com.autovrse.vrsebuilder.mcptools.Editor.Models
{
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
    }
}
