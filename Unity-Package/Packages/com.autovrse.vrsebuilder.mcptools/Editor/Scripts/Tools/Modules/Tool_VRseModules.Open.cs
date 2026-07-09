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
using OpenSceneMode = UnityEditor.SceneManagement.OpenSceneMode;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Modules
{
    public partial class Tool_VRseModules
    {
        [AiTool("vrse-module-open", Title = "VRseBuilder / Module / Open")]
        [Description("Open a module experience dev scene and load its art scene additively when configured.")]
        public VrseOpenSceneResult OpenModule
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null,
            [Description("Module ID to open. Use moduleId or moduleName.")]
            string? moduleId = null,
            [Description("Module name to open. Use moduleId or moduleName.")]
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
                ResolveModuleAndExperience(projectName, moduleId, moduleName, experienceId, experienceName, experienceType,
                    out string resolvedProjectName, out VrseConfigModuleInfo module, out VrseConfigExperienceInfo experience);

                if (string.IsNullOrEmpty(experience.DevScene))
                    throw new InvalidOperationException($"Experience '{experience.Name}' does not have a dev scene configured.");

                if (!File.Exists(experience.DevScene))
                    throw new FileNotFoundException("Dev scene file does not exist.", experience.DevScene);

                UnityScene devScene = EditorSceneManager.OpenScene(experience.DevScene, OpenSceneMode.Single);
                bool artSceneLoaded = false;
                if (!string.IsNullOrEmpty(experience.ArtScene) && File.Exists(experience.ArtScene))
                {
                    EditorSceneManager.OpenScene(experience.ArtScene, OpenSceneMode.Additive);
                    artSceneLoaded = true;
                }

                return BuildSceneResult(resolvedProjectName, module, experience, experience.DevScene, devScene.name, artSceneLoaded);
            });
        }

        [AiTool("vrse-module-open-art-scene", Title = "VRseBuilder / Module / Open Art Scene")]
        [Description("Open a module experience art scene additively.")]
        public VrseOpenSceneResult OpenArtScene
        (
            [Description("Optional project name. Uses the selected project when omitted.")]
            string? projectName = null,
            [Description("Module ID to open. Use moduleId or moduleName.")]
            string? moduleId = null,
            [Description("Module name to open. Use moduleId or moduleName.")]
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
                ResolveModuleAndExperience(projectName, moduleId, moduleName, experienceId, experienceName, experienceType,
                    out string resolvedProjectName, out VrseConfigModuleInfo module, out VrseConfigExperienceInfo experience);

                if (string.IsNullOrEmpty(experience.ArtScene))
                    throw new InvalidOperationException($"Experience '{experience.Name}' does not have an art scene configured.");

                if (!File.Exists(experience.ArtScene))
                    throw new FileNotFoundException("Art scene file does not exist.", experience.ArtScene);

                UnityScene artScene = EditorSceneManager.OpenScene(experience.ArtScene, OpenSceneMode.Additive);
                return BuildSceneResult(resolvedProjectName, module, experience, experience.ArtScene, artScene.name, artSceneLoaded: true);
            });
        }

        private static VrseOpenSceneResult BuildSceneResult(string projectName, VrseConfigModuleInfo module, VrseConfigExperienceInfo experience, string scenePath, string openedSceneName, bool artSceneLoaded)
        {
            return new VrseOpenSceneResult
            {
                Success = true,
                ProjectName = projectName,
                ModuleId = module.ModuleId,
                ModuleName = module.Name,
                ExperienceId = experience.ExperienceId,
                ExperienceName = experience.Name,
                ExperienceType = experience.Type,
                ScenePath = scenePath,
                OpenedSceneName = openedSceneName,
                ArtScenePath = experience.ArtScene,
                ArtSceneLoaded = artSceneLoaded
            };
        }
    }
}
