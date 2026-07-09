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
using UnityEditor;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Modules
{
    public partial class Tool_VRseModules
    {
        [AiTool("vrse-module-create-experience", Title = "VRseBuilder / Module / Create Experience")]
        [Description("Create a VRseBuilder experience dev scene through the SDK project-window controller.")]
        public VrseCreateExperienceResult CreateExperience
        (
            [Description("Module name that will contain the experience.")]
            string moduleName,
            [Description("Experience name to create.")]
            string experienceName,
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null,
            [Description("Story JSON URL. The current SDK also accepts an empty/local value and creates an empty story.")]
            string? jsonFileUrl = null,
            [Description("Optional backend module ID for config tracking.")]
            string? moduleId = null,
            [Description("Optional backend experience ID for config tracking.")]
            string? experienceId = null,
            [Description("Experience type. Usually Training or Evaluation.")]
            string experienceType = "Training"
        )
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                throw new ArgumentException("Module name is required.", nameof(moduleName));

            if (string.IsNullOrWhiteSpace(experienceName))
                throw new ArgumentException("Experience name is required.", nameof(experienceName));

            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = ResolveProjectName(projectName);
                Type controllerType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseProjectWindowController")
                    ?? throw new InvalidOperationException("VRseProjectWindowController was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                Type experienceTypeType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.ModuleData+ExperienceType")
                    ?? throw new InvalidOperationException("ModuleData.ExperienceType was not found. Ensure the VRseBuilder SDK is installed and compiled.");

                object parsedExperienceType = Enum.Parse(experienceTypeType, experienceType, ignoreCase: true);
                object controller = VrseReflection.CreateInstance(controllerType)
                    ?? throw new InvalidOperationException("Could not create VRseProjectWindowController.");

                VrseReflection.InvokeInstance(controller, "CreateExperienceDevScene",
                    resolvedProjectName,
                    moduleName.Trim(),
                    experienceName.Trim(),
                    jsonFileUrl ?? string.Empty,
                    moduleId,
                    experienceId,
                    parsedExperienceType);

                AssetDatabase.Refresh();

                VrseConfigModuleInfo? module = VrseProjectDiscovery.FindModule(resolvedProjectName, moduleId, moduleName);
                VrseConfigExperienceInfo? experience = module != null
                    ? VrseProjectDiscovery.FindExperience(module, experienceId, experienceName, experienceType)
                    : null;

                return new VrseCreateExperienceResult
                {
                    Success = true,
                    ProjectName = resolvedProjectName,
                    JsonFileUrl = jsonFileUrl ?? string.Empty,
                    ModuleName = moduleName.Trim(),
                    ExperienceName = experienceName.Trim(),
                    ExperienceType = experienceType,
                    CreationStatus = module != null && experience != null ? BuildExperienceStatus(resolvedProjectName, module, experience) : null,
                    Warnings = module == null || experience == null
                        ? new System.Collections.Generic.List<string> { "Experience creation ran, but the experience could not be resolved from RoomManagerConfig." }
                        : new System.Collections.Generic.List<string>()
                };
            });
        }
    }
}
