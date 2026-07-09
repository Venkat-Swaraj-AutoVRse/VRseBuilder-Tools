/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Build
{
    public partial class Tool_VRseBuild
    {
        [AiTool("vrse-build-open-tool", Title = "VRseBuilder / Build / Open Tool")]
        [Description("Open the VRseBuilder Build Tool editor window when the SDK is installed.")]
        public VrseBuildWindowResult OpenBuildTool()
        {
            return MainThread.Instance.Run(() =>
            {
                Type windowType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.BuildTool.VRseBuildToolWindow")
                    ?? throw new InvalidOperationException("VRseBuildToolWindow was not found. Ensure VRseBuilder SDK editor tools are installed and compiled.");
                VrseReflection.InvokeStatic(windowType, "ShowWindow");
                return new VrseBuildWindowResult { Success = true, Window = "VRse Build Tool" };
            });
        }

        [AiTool("vrse-build-module-set-include", Title = "VRseBuilder / Build / Module Set Include")]
        [Description("Set whether a VRseBuilder module is included in build scene preparation.")]
        public VrseModuleIncludeResult ModuleSetIncludeInBuild(string moduleName, bool includeInBuild = true, string? projectName = null)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("moduleName is required.", nameof(moduleName));

            return MainThread.Instance.Run(() =>
            {
                string resolvedProject = ResolveProjectName(projectName);
                VrseConfigModuleInfo module = VrseProjectDiscovery.FindModule(resolvedProject, moduleName: moduleName)
                    ?? throw new InvalidOperationException($"Module '{moduleName}' was not found in project '{resolvedProject}'.");
                UnityEngine.Object config = VrseProjectDiscovery.LoadRoomManagerConfig(resolvedProject)
                    ?? throw new InvalidOperationException($"RoomManagerConfig was not found for project '{resolvedProject}'.");

                SetModuleInclude(config, module.Index, includeInBuild);
                return new VrseModuleIncludeResult
                {
                    Success = true,
                    ProjectName = resolvedProject,
                    ModuleName = module.Name,
                    ModuleId = module.ModuleId,
                    IncludeInBuild = includeInBuild
                };
            });
        }

        [AiTool("vrse-build-start", Title = "VRseBuilder / Build / Start")]
        [Description("Prepare build scenes from RoomManagerConfig and run a synchronous Unity build.")]
        public VrseBuildStartResult BuildStart(string buildPath, string? projectName = null, bool cleanBuild = false)
        {
            if (string.IsNullOrWhiteSpace(buildPath))
                throw new ArgumentException("buildPath is required.", nameof(buildPath));

            return MainThread.Instance.Run(() =>
            {
                string resolvedProject = ResolveProjectName(projectName);
                List<string> buildScenes = PrepareBuildScenes(resolvedProject);
                if (buildScenes.Count == 0)
                    throw new InvalidOperationException("No enabled scenes were found for build.");

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = buildScenes.ToArray(),
                    locationPathName = buildPath,
                    target = EditorUserBuildSettings.activeBuildTarget,
                    options = cleanBuild ? BuildOptions.CleanBuildCache : BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                return new VrseBuildStartResult
                {
                    Success = report.summary.result == BuildResult.Succeeded,
                    Result = report.summary.result.ToString(),
                    TotalErrors = report.summary.totalErrors,
                    TotalWarnings = report.summary.totalWarnings,
                    OutputPath = report.summary.outputPath,
                    Scenes = buildScenes
                };
            });
        }

        [AiTool("vrse-build-status", Title = "VRseBuilder / Build / Status")]
        [Description("Return high-level build readiness status.")]
        public VrseBuildStatusResult BuildStatus()
        {
            return new VrseBuildStatusResult
            {
                Status = "ready",
                Message = "Unity Editor builds triggered by MCP are synchronous and block the main thread. If the Editor is responsive, no build is currently running."
            };
        }

        private static string ResolveProjectName(string? projectName)
        {
            string resolved = string.IsNullOrWhiteSpace(projectName) ? VrseEditorPrefs.SelectedProject : projectName.Trim();
            if (string.IsNullOrEmpty(resolved))
                throw new InvalidOperationException("No VRseBuilder project is selected. Use vrse-project-select first or pass projectName.");
            return resolved;
        }

        private static void SetModuleInclude(UnityEngine.Object config, int moduleIndex, bool include)
        {
            var serialized = new SerializedObject(config);
            SerializedProperty modules = serialized.FindProperty("experiences")
                ?? throw new InvalidOperationException("RoomManagerConfig.experiences could not be found.");
            if (moduleIndex < 0 || moduleIndex >= modules.arraySize)
                throw new ArgumentOutOfRangeException(nameof(moduleIndex));

            SerializedProperty module = modules.GetArrayElementAtIndex(moduleIndex);
            SerializedProperty property = module.FindPropertyRelative("_includeInBuild") ?? module.FindPropertyRelative("IncludeInBuild")
                ?? throw new InvalidOperationException("Module IncludeInBuild serialized field could not be found.");
            property.boolValue = include;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static List<string> PrepareBuildScenes(string projectName)
        {
            var scenes = new List<string>();
            UnityEngine.Object config = VrseProjectDiscovery.LoadRoomManagerConfig(projectName)
                ?? throw new InvalidOperationException($"RoomManagerConfig was not found for project '{projectName}'.");
            string menuScene = VrseProjectDiscovery.ReadStringProperty(config, "MainMenuScene");
            AddSceneIfExists(scenes, menuScene);

            foreach (VrseConfigModuleInfo module in VrseProjectDiscovery.ListConfigModules(projectName).Where(module => module.IncludeInBuild))
            {
                foreach (VrseConfigExperienceInfo experience in module.Experiences)
                {
                    AddSceneIfExists(scenes, experience.DevScene);
                    AddSceneIfExists(scenes, experience.ArtScene);
                }
            }

            EditorBuildSettings.scenes = scenes.Select(scene => new EditorBuildSettingsScene(scene, true)).ToArray();
            return scenes;
        }

        private static void AddSceneIfExists(List<string> scenes, string path)
        {
            if (string.IsNullOrEmpty(path) || scenes.Contains(path) || !File.Exists(path))
                return;
            scenes.Add(path);
        }
    }
}
