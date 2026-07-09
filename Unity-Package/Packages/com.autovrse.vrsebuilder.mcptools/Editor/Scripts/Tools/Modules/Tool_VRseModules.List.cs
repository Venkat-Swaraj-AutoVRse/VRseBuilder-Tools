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

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Modules
{
    public partial class Tool_VRseModules
    {
        [AiTool("vrse-module-list", Title = "VRseBuilder / Module / List")]
        [Description("List modules and configured experiences from a project's RoomManagerConfig asset.")]
        public VrseListModulesResult ListModules
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

                var config = VrseProjectDiscovery.LoadRoomManagerConfig(resolvedProjectName);
                if (config == null)
                    throw new InvalidOperationException($"RoomManagerConfig was not found for project '{resolvedProjectName}'.");

                return new VrseListModulesResult
                {
                    ProjectName = resolvedProjectName,
                    ConfigSourceAvailable = true,
                    ConfigModules = VrseProjectDiscovery.ListConfigModules(resolvedProjectName)
                };
            });
        }
    }
}
