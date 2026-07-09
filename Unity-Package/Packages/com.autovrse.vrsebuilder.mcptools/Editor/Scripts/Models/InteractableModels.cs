/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

namespace com.autovrse.vrsebuilder.mcptools.Editor.Models
{
    public sealed class VrseInteractableConvertResult
    {
        public bool Success { get; set; }
        public string ResolvedObject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class VrseRotatorAnalyzeResult
    {
        public bool Success { get; set; }
        public string Json { get; set; } = string.Empty;
    }

    public sealed class VrseRotatorCreateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RootObjectName { get; set; } = string.Empty;
        public int InstanceId { get; set; }
        public string RawResult { get; set; } = string.Empty;
    }
}
