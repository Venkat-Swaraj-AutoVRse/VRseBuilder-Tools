/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Modules
{
    public partial class Tool_VRseModules
    {
        private static string ResolveProjectName(string? projectName)
        {
            string resolvedProjectName = string.IsNullOrWhiteSpace(projectName) ? VrseEditorPrefs.SelectedProject : projectName.Trim();
            if (string.IsNullOrEmpty(resolvedProjectName))
                throw new InvalidOperationException("No VRseBuilder project is selected. Use vrse-project-select first or pass projectName.");

            return resolvedProjectName;
        }

        private static void ResolveModuleAndExperience(
            string? projectName,
            string? moduleId,
            string? moduleName,
            string? experienceId,
            string? experienceName,
            string? experienceType,
            out string resolvedProjectName,
            out VrseConfigModuleInfo module,
            out VrseConfigExperienceInfo experience)
        {
            resolvedProjectName = ResolveProjectName(projectName);
            module = VrseProjectDiscovery.FindModule(resolvedProjectName, moduleId, moduleName)
                ?? throw new InvalidOperationException("Module not found. Provide moduleId or moduleName.");

            experience = VrseProjectDiscovery.FindExperience(module, experienceId, experienceName, experienceType)
                ?? throw new InvalidOperationException("Experience not found. Provide experienceId, experienceName, or experienceType.");
        }
    }
}
