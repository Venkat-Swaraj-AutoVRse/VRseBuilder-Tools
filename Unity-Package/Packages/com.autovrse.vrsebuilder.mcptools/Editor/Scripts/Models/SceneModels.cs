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
    public sealed class VrseQueryObjectsListResult
    {
        public bool Success { get; set; }
        public int Count { get; set; }
        public List<VrseQueryObjectInfo> QueryObjects { get; set; } = new();
    }

    public sealed class VrseQueryObjectInfo
    {
        public string QueryName { get; set; } = string.Empty;
        public int Id { get; set; }
        public bool IsIdValid { get; set; }
        public string GameObjectName { get; set; } = string.Empty;
        public string GameObjectPath { get; set; } = string.Empty;
        public bool ActiveInHierarchy { get; set; }
        public List<string> VrseComponents { get; set; } = new();
    }

    public sealed class VrseBuildingBlockListResult
    {
        public bool Success { get; set; }
        public int Count { get; set; }
        public List<VrseBuildingBlockInfo> Blocks { get; set; } = new();
    }

    public sealed class VrseBuildingBlockInfo
    {
        public string BlockName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string ActionName { get; set; } = string.Empty;
        public string TriggerName { get; set; } = string.Empty;
    }

    public sealed class VrseInstantiateBuildingBlockResult
    {
        public bool Success { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public int InstanceId { get; set; }
    }

    public sealed class VrseSceneActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
