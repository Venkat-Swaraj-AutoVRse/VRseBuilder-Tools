/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.ComponentModel;
using System.IO;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Modules
{
    public partial class Tool_VRseModules
    {
        [AiTool("vrse-module-experience-status", Title = "VRseBuilder / Module / Experience Status")]
        [Description("Return whether a configured module experience has story JSON, dev scene, and art scene files.")]
        public VrseExperienceCreationStatusResult GetExperienceCreationStatus
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null,
            [Description("Module ID. Use moduleId or moduleName.")]
            string? moduleId = null,
            [Description("Module name. Use moduleId or moduleName.")]
            string? moduleName = null,
            [Description("Optional experience ID.")]
            string? experienceId = null,
            [Description("Optional experience name.")]
            string? experienceName = null,
            [Description("Optional experience type, usually Training or Evaluation.")]
            string? experienceType = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProjectName = ResolveProjectName(projectName);
                VrseConfigModuleInfo? module = VrseProjectDiscovery.FindModule(resolvedProjectName, moduleId, moduleName);
                if (module == null)
                    return new VrseExperienceCreationStatusResult { ProjectName = resolvedProjectName, ModuleFound = false };

                VrseConfigExperienceInfo? experience = VrseProjectDiscovery.FindExperience(module, experienceId, experienceName, experienceType);
                if (experience == null)
                {
                    return new VrseExperienceCreationStatusResult
                    {
                        ProjectName = resolvedProjectName,
                        ModuleFound = true,
                        ExperienceFound = false,
                        ModuleId = module.ModuleId,
                        ModuleName = module.Name
                    };
                }

                return BuildExperienceStatus(resolvedProjectName, module, experience);
            });
        }

        private static VrseExperienceCreationStatusResult BuildExperienceStatus(string projectName, VrseConfigModuleInfo module, VrseConfigExperienceInfo experience)
        {
            string storyJsonAbsolutePath = GetAbsoluteStoryJsonPath(experience.StoryJsonPath);
            bool storyJsonExists = !string.IsNullOrEmpty(storyJsonAbsolutePath) && File.Exists(storyJsonAbsolutePath);
            bool devSceneExists = !string.IsNullOrEmpty(experience.DevScene) && File.Exists(experience.DevScene);
            bool artSceneExists = !string.IsNullOrEmpty(experience.ArtScene) && File.Exists(experience.ArtScene);

            return new VrseExperienceCreationStatusResult
            {
                ProjectName = projectName,
                ModuleFound = true,
                ExperienceFound = true,
                ModuleId = module.ModuleId,
                ModuleName = module.Name,
                ExperienceId = experience.ExperienceId,
                ExperienceName = experience.Name,
                ExperienceType = experience.Type,
                StoryJsonPath = experience.StoryJsonPath,
                StoryJsonAbsolutePath = storyJsonAbsolutePath,
                StoryJsonExists = storyJsonExists,
                DevScenePath = experience.DevScene,
                DevSceneExists = devSceneExists,
                ArtScenePath = experience.ArtScene,
                ArtSceneExists = artSceneExists,
                IsFullyConfigured = storyJsonExists && devSceneExists && artSceneExists
            };
        }

        private static string GetAbsoluteStoryJsonPath(string storyJsonPath)
        {
            if (string.IsNullOrEmpty(storyJsonPath))
                return string.Empty;

            if (Path.IsPathRooted(storyJsonPath))
                return storyJsonPath;

            return Path.Combine(Application.streamingAssetsPath, storyJsonPath.TrimStart('/', '\\')).Replace("\\", "/");
        }
    }
}
