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
    public sealed class VrseOpenAssetResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
    }

    public sealed class VrseOpenWindowResult
    {
        public bool Success { get; set; }
        public string Window { get; set; } = string.Empty;
    }

    public sealed class VrseOpenSceneResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string OpenedSceneName { get; set; } = string.Empty;
        public string ScenePath { get; set; } = string.Empty;
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ExperienceId { get; set; } = string.Empty;
        public string ExperienceName { get; set; } = string.Empty;
        public string ExperienceType { get; set; } = string.Empty;
        public string ArtScenePath { get; set; } = string.Empty;
        public bool ArtSceneLoaded { get; set; }
    }

    public sealed class VrseProjectSettingsActionResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public bool NewSettingsCreated { get; set; }
        public object? RawResult { get; set; }
    }

    public sealed class VrseExperienceCreationStatusResult
    {
        public string ProjectName { get; set; } = string.Empty;
        public bool ModuleFound { get; set; }
        public bool ExperienceFound { get; set; }
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ExperienceId { get; set; } = string.Empty;
        public string ExperienceName { get; set; } = string.Empty;
        public string ExperienceType { get; set; } = string.Empty;
        public string StoryJsonPath { get; set; } = string.Empty;
        public string StoryJsonAbsolutePath { get; set; } = string.Empty;
        public bool StoryJsonExists { get; set; }
        public string DevScenePath { get; set; } = string.Empty;
        public bool DevSceneExists { get; set; }
        public string ArtScenePath { get; set; } = string.Empty;
        public bool ArtSceneExists { get; set; }
        public bool IsFullyConfigured { get; set; }
    }

    public sealed class VrseCreateExperienceResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string JsonFileUrl { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ExperienceName { get; set; } = string.Empty;
        public string ExperienceType { get; set; } = string.Empty;
        public VrseExperienceCreationStatusResult? CreationStatus { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
