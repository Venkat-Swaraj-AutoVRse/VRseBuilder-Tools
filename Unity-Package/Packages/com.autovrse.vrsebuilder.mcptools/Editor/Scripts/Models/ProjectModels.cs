/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.Collections.Generic;
using System.ComponentModel;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Models
{
    public sealed class VrseProjectStatusResult
    {
        [Description("Whether the VRseBuilder backend auth utility reports an active login.")]
        public bool LoggedIn { get; set; }

        [Description("Logged-in VRseBuilder user name when available.")]
        public string UserName { get; set; } = string.Empty;

        [Description("Configured VRseBuilder backend base URL when available.")]
        public string BaseUrl { get; set; } = string.Empty;

        [Description("Project selected for VRseBuilder AI tool operations.")]
        public string SelectedProject { get; set; } = string.Empty;

        [Description("Active Unity scene name.")]
        public string ActiveSceneName { get; set; } = string.Empty;

        [Description("Active Unity scene asset path.")]
        public string ActiveScenePath { get; set; } = string.Empty;

        [Description("Whether the selected project has a RoomManagerConfig asset.")]
        public bool HasSelectedProjectConfig { get; set; }
    }

    public sealed class VrseLocalProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool HasRoomManagerConfig { get; set; }
        public string MainMenuScene { get; set; } = string.Empty;
        public bool HasMenuScene { get; set; }
    }

    public sealed class VrseListProjectsResult
    {
        public bool LoggedIn { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string SelectedProject { get; set; } = string.Empty;
        public List<VrseLocalProjectInfo> LocalProjects { get; set; } = new();
    }

    public sealed class VrseSelectProjectResult
    {
        public bool Success { get; set; }
        public string SelectedProject { get; set; } = string.Empty;
    }

    public sealed class VrseSelectedProjectResult
    {
        public string SelectedProject { get; set; } = string.Empty;
        public bool HasRoomManagerConfig { get; set; }
        public bool HasMenuScene { get; set; }
    }

    public sealed class VrseProjectConfigInfo
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string MainMenuScene { get; set; } = string.Empty;
        public bool LiveLinkEnabled { get; set; }
        public bool UseCustomAvatars { get; set; }
        public bool StepNavigationDataEnabled { get; set; }
        public int ModuleCount { get; set; }
    }
}
