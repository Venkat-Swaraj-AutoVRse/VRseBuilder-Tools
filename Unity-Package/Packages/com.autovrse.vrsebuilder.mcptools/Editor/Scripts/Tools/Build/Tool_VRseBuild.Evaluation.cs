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
using System.Reflection;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Build
{
    public partial class Tool_VRseBuild
    {
        [AiTool("vrse-evaluation-create-from-training", Title = "VRseBuilder / Evaluation / Create From Training")]
        [Description("Create an evaluation scene and story from a configured training experience using the VRseBuilder automation editor.")]
        public VrseEvaluationCreateResult CreateEvaluationFromTraining(
            string? projectName = null,
            string? moduleId = null,
            string? moduleName = null,
            string? experienceId = null,
            string? experienceName = null,
            string evaluationSceneName = "",
            string evaluationStoryName = "",
            string storyDefaults = "{\"storyMode\":\"Evaluation\",\"maxAttempts\":\"3\",\"useDefaultMaxAttempts\":\"false\"}",
            string evaluationType = "MomentLifeCycle",
            bool showAutoToastForWrongAction = true,
            bool showAutoToastForRightAction = true,
            float rightActionToastMessageDisplayTime = 2.5f,
            float mistakeCoolDownTime = 2f,
            int debugMistakesCountToPass = 3)
        {
            return MainThread.Instance.Run(() =>
            {
                string resolvedProject = ResolveProjectName(projectName);
                VrseConfigModuleInfo module = VrseProjectDiscovery.FindModule(resolvedProject, moduleId, moduleName)
                    ?? throw new InvalidOperationException("Module not found. Provide moduleId or moduleName.");
                VrseConfigExperienceInfo experience = VrseProjectDiscovery.FindExperience(module, experienceId, experienceName, "Training")
                    ?? throw new InvalidOperationException("Training experience not found. Provide experienceId or experienceName.");
                if (string.IsNullOrEmpty(experience.DevScene) || !File.Exists(experience.DevScene))
                    throw new FileNotFoundException("Training dev scene file does not exist.", experience.DevScene);

                EditorSceneManager.OpenScene(experience.DevScene, OpenSceneMode.Single);
                object storyCreator = VrseStoryDiscovery.FindStoryCreator();
                string storyJsonPath = GetAbsoluteStoryJsonPath(experience.StoryJsonPath);
                if (string.IsNullOrEmpty(storyJsonPath) || !File.Exists(storyJsonPath))
                    throw new FileNotFoundException("Training story JSON file does not exist.", storyJsonPath);

                if (string.IsNullOrWhiteSpace(evaluationSceneName))
                    evaluationSceneName = CreateDefaultEvaluationName(experience.Name, "Evaluation_Scene");
                if (string.IsNullOrWhiteSpace(evaluationStoryName))
                    evaluationStoryName = CreateDefaultEvaluationName(experience.Name, "Evaluation_JSON");

                Type windowType = VrseTypeResolver.FindType("VRseBuilder.Tools.Editor.VRseEvaluationAutomationToolEditor")
                    ?? throw new InvalidOperationException("VRseEvaluationAutomationToolEditor was not found. Ensure VRseBuilder automation tools are installed and compiled.");
                EditorWindow window = EditorWindow.GetWindow(windowType, false, "Evaluation Automation Tool");

                SetPrivate(windowType, window, "_storyCreator", storyCreator);
                SetPrivate(windowType, window, "_originalStoryPath", storyJsonPath);
                SetPrivate(windowType, window, "_currentSceneName", Path.GetFileNameWithoutExtension(experience.DevScene));
                SetPrivate(windowType, window, "_currentStoryName", VrseReflection.GetString(storyCreator, "_fileName"));
                SetPrivate(windowType, window, "_evaluationSceneName", evaluationSceneName);
                SetPrivate(windowType, window, "_evaluationStoryName", evaluationStoryName);
                SetPrivate(windowType, window, "_storyDefaults", storyDefaults);
                SetPrivate(windowType, window, "_useExistingEvaluation", false);
                SetPrivate(windowType, window, "_showAutoToastMessageForWrongAction", showAutoToastForWrongAction);
                SetPrivate(windowType, window, "_showAutoToastMessageForRightAction", showAutoToastForRightAction);
                SetPrivate(windowType, window, "_rightActionToastMessageDisplayTime", rightActionToastMessageDisplayTime);
                SetPrivate(windowType, window, "_mistakeCoolDownTime", mistakeCoolDownTime);
                SetPrivate(windowType, window, "_debugMistakesCountToPass", debugMistakesCountToPass);

                Type? evaluationTypeType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.Enums+EvaluationType");
                if (evaluationTypeType != null && Enum.TryParse(evaluationTypeType, evaluationType, true, out object parsedEvaluationType))
                    SetPrivate(windowType, window, "_evaluationType", parsedEvaluationType);

                MethodInfo method = windowType.GetMethod("CreateEvaluationScene", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("CreateEvaluationScene method was not found.");
                method.Invoke(window, null);

                string evaluationScenePath = Path.Combine(Path.GetDirectoryName(experience.DevScene) ?? string.Empty, evaluationSceneName + ".unity").Replace("\\", "/");
                string evaluationStoryPath = Path.Combine(Path.GetDirectoryName(storyJsonPath) ?? string.Empty, evaluationStoryName + ".json").Replace("\\", "/");
                return new VrseEvaluationCreateResult
                {
                    Success = true,
                    EvaluationScene = evaluationScenePath,
                    EvaluationStory = evaluationStoryPath,
                    StoryDefaults = storyDefaults
                };
            });
        }

        private static void SetPrivate(Type type, object target, string fieldName, object? value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {type.Name}.");
            field.SetValue(target, value);
        }

        private static string CreateDefaultEvaluationName(string baseName, string suffix)
        {
            if (string.IsNullOrEmpty(baseName))
                return $"Evaluation_{suffix}";
            return baseName.EndsWith("_Training", StringComparison.OrdinalIgnoreCase)
                ? baseName.Substring(0, baseName.Length - "_Training".Length) + "_" + suffix
                : baseName + "_" + suffix;
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
