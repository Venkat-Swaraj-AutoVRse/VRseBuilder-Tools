/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.ComponentModel;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-get-selected", Title = "VRseBuilder / Project / Get Selected")]
        [Description("Get the currently selected VRseBuilder project and whether its config/menu scene exist.")]
        public VrseSelectedProjectResult GetSelectedProject()
        {
            return MainThread.Instance.Run(() =>
            {
                string projectName = VrseEditorPrefs.SelectedProject;
                var config = !string.IsNullOrEmpty(projectName) ? VrseProjectDiscovery.LoadRoomManagerConfig(projectName) : null;
                string mainMenuScene = config != null ? VrseProjectDiscovery.ReadStringProperty(config, "MainMenuScene") : string.Empty;

                return new VrseSelectedProjectResult
                {
                    SelectedProject = projectName,
                    HasRoomManagerConfig = config != null,
                    HasMenuScene = !string.IsNullOrEmpty(mainMenuScene) && System.IO.File.Exists(mainMenuScene)
                };
            });
        }
    }
}
