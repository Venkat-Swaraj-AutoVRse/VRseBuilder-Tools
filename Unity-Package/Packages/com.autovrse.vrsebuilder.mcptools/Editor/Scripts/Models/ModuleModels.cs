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
    public sealed class VrseListModulesResult
    {
        public string ProjectName { get; set; } = string.Empty;
        public bool ConfigSourceAvailable { get; set; }
        public List<VrseConfigModuleInfo> ConfigModules { get; set; } = new();
    }

    public sealed class VrseConfigModuleInfo
    {
        public int Index { get; set; }
        public string ModuleId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IncludeInBuild { get; set; }
        public List<VrseConfigExperienceInfo> Experiences { get; set; } = new();
    }

    public sealed class VrseConfigExperienceInfo
    {
        public int Index { get; set; }
        public string ExperienceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string DevScene { get; set; } = string.Empty;
        public string ArtScene { get; set; } = string.Empty;
        public string StoryJsonPath { get; set; } = string.Empty;
        public bool HasDevScene { get; set; }
        public bool HasArtScene { get; set; }
    }
}
