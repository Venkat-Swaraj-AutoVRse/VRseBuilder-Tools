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
using UnityEditor;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-open-room-manager-config", Title = "VRseBuilder / Project / Open RoomManagerConfig")]
        [Description("Open and select the RoomManagerConfig asset for a VRseBuilder project.")]
        public VrseOpenAssetResult OpenRoomManagerConfig
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = ResolveProjectName(projectName);
                UnityEngine.Object config = VrseProjectDiscovery.LoadRoomManagerConfig(resolvedProjectName)
                    ?? throw new InvalidOperationException($"RoomManagerConfig was not found for project '{resolvedProjectName}'.");

                Selection.activeObject = config;
                EditorUtility.OpenPropertyEditor(config);

                return new VrseOpenAssetResult
                {
                    Success = true,
                    ProjectName = resolvedProjectName,
                    AssetName = config.name
                };
            });
        }
    }
}
