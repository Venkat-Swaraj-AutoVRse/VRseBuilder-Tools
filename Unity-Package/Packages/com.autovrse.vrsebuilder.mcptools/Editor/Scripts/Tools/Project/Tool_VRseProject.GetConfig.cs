/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-get-config", Title = "VRseBuilder / Project / Get Config")]
        [Description("Read high-level fields from the selected project's RoomManagerConfig asset.")]
        public VrseProjectConfigInfo GetProjectConfig
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = string.IsNullOrWhiteSpace(projectName) ? VrseEditorPrefs.SelectedProject : projectName.Trim();
                if (string.IsNullOrEmpty(resolvedProjectName))
                    throw new InvalidOperationException("No VRseBuilder project is selected. Use vrse-project-select first or pass projectName.");

                VrseProjectConfigInfo? config = VrseProjectDiscovery.ReadProjectConfig(resolvedProjectName);
                if (config == null)
                    throw new InvalidOperationException($"RoomManagerConfig was not found for project '{resolvedProjectName}'.");

                return config;
            });
        }
    }
}
