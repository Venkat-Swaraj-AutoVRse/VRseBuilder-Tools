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
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Project
{
    public partial class Tool_VRseProject
    {
        [AiTool("vrse-project-ensure-settings", Title = "VRseBuilder / Project / Ensure Settings")]
        [Description("Create missing VRseBuilder project settings assets using the SDK controller when available.")]
        public VrseProjectSettingsActionResult EnsureProjectSettings
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = ResolveProjectName(projectName);
                Type controllerType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseProjectWindowProjectSettingsController")
                    ?? throw new InvalidOperationException("VRseProjectWindowProjectSettingsController was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                object? raw = VrseReflection.InvokeStatic(controllerType, "EnsureProjectSettingsExist", resolvedProjectName);
                return new VrseProjectSettingsActionResult
                {
                    Success = true,
                    ProjectName = resolvedProjectName,
                    NewSettingsCreated = raw is bool created && created,
                    RawResult = raw
                };
            });
        }

        [AiTool("vrse-project-apply-settings", Title = "VRseBuilder / Project / Apply Settings")]
        [Description("Apply VRseBuilder project settings using the SDK auto-apply controller when available.")]
        public VrseProjectSettingsActionResult ApplyProjectSettings
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = ResolveProjectName(projectName);
                Type autoApplyType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseProjectConfigAutoApply")
                    ?? throw new InvalidOperationException("VRseProjectConfigAutoApply was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                object? raw = VrseReflection.InvokeStatic(autoApplyType, "AutoApplyAllSettingsOnProjectChange", resolvedProjectName);
                return new VrseProjectSettingsActionResult
                {
                    Success = true,
                    ProjectName = resolvedProjectName,
                    RawResult = raw
                };
            });
        }

        private static string ResolveProjectName(string? projectName)
        {
            string resolvedProjectName = string.IsNullOrWhiteSpace(projectName) ? VrseEditorPrefs.SelectedProject : projectName.Trim();
            if (string.IsNullOrEmpty(resolvedProjectName))
                throw new InvalidOperationException("No VRseBuilder project is selected. Use vrse-project-select first or pass projectName.");

            return resolvedProjectName;
        }
    }
}
