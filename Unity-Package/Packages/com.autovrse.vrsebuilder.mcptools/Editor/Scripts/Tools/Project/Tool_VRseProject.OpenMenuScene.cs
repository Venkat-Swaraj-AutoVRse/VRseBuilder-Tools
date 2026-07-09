/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using System.IO;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-open-menu-scene", Title = "VRseBuilder / Project / Open Menu Scene")]
        [Description("Open the selected project's configured main menu scene.")]
        public VrseOpenSceneResult OpenMenuScene
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

                string menuScene = VrseProjectDiscovery.ReadStringProperty(config, "MainMenuScene");
                if (string.IsNullOrEmpty(menuScene))
                    throw new InvalidOperationException($"Main menu scene is not configured for project '{resolvedProjectName}'.");

                if (!File.Exists(menuScene))
                    throw new FileNotFoundException("Main menu scene file does not exist.", menuScene);

                Scene scene = EditorSceneManager.OpenScene(menuScene, OpenSceneMode.Single);
                return new VrseOpenSceneResult
                {
                    Success = true,
                    ProjectName = resolvedProjectName,
                    ScenePath = menuScene,
                    OpenedSceneName = scene.name
                };
            });
        }
    }
}
