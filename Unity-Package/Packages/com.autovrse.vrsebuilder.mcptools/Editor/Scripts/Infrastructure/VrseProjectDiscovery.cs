/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseProjectDiscovery
    {
        public static string StudioProjectsRoot => Path.Combine(Application.dataPath, "StudioProjects");

        public static List<VrseLocalProjectInfo> ListLocalProjects()
        {
            var projects = new List<VrseLocalProjectInfo>();
            if (!Directory.Exists(StudioProjectsRoot))
                return projects;

            foreach (string directory in Directory.GetDirectories(StudioProjectsRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                string name = Path.GetFileName(directory);
                UnityEngine.Object? config = LoadRoomManagerConfig(name);
                string mainMenuScene = config != null ? ReadStringProperty(config, "MainMenuScene") : string.Empty;

                projects.Add(new VrseLocalProjectInfo
                {
                    Name = name,
                    HasRoomManagerConfig = config != null,
                    MainMenuScene = mainMenuScene,
                    HasMenuScene = !string.IsNullOrEmpty(mainMenuScene) && File.Exists(mainMenuScene)
                });
            }

            return projects;
        }

        public static UnityEngine.Object? LoadRoomManagerConfig(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return null;

            string assetPath = $"Assets/StudioProjects/{projectName}/ProjectSettings/RoomManagerConfig_{projectName}.asset";
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        public static VrseProjectConfigInfo? ReadProjectConfig(string projectName)
        {
            UnityEngine.Object? config = LoadRoomManagerConfig(projectName);
            if (config == null)
                return null;

            return new VrseProjectConfigInfo
            {
                ProjectName = projectName,
                ProjectId = ReadStringProperty(config, "ProjectID"),
                MainMenuScene = ReadStringProperty(config, "MainMenuScene"),
                LiveLinkEnabled = ReadBoolProperty(config, "LiveLinkEnabled"),
                UseCustomAvatars = ReadBoolProperty(config, "UseCustomAvatars"),
                StepNavigationDataEnabled = ReadBoolProperty(config, "StepNavigationDataEnabled"),
                ModuleCount = ReadArraySize(config, "experiences")
            };
        }

        public static List<VrseConfigModuleInfo> ListConfigModules(string projectName)
        {
            var modules = new List<VrseConfigModuleInfo>();
            UnityEngine.Object? config = LoadRoomManagerConfig(projectName);
            if (config == null)
                return modules;

            var serialized = new SerializedObject(config);
            SerializedProperty? experiences = serialized.FindProperty("experiences");
            if (experiences == null || !experiences.isArray)
                return modules;

            for (int i = 0; i < experiences.arraySize; i++)
            {
                SerializedProperty moduleProperty = experiences.GetArrayElementAtIndex(i);
                var module = new VrseConfigModuleInfo
                {
                    Index = i,
                    ModuleId = ReadRelativeString(moduleProperty, "ModuleId"),
                    Name = ReadModuleName(moduleProperty, i),
                    IncludeInBuild = ReadRelativeBool(moduleProperty, "IncludeInBuild"),
                    Experiences = ReadExperiences(moduleProperty)
                };
                modules.Add(module);
            }

            return modules;
        }

        public static string ReadStringProperty(UnityEngine.Object target, string propertyName)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty? property = serialized.FindProperty(propertyName);
            return property?.propertyType == SerializedPropertyType.String ? property.stringValue : string.Empty;
        }

        private static bool ReadBoolProperty(UnityEngine.Object target, string propertyName)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty? property = serialized.FindProperty(propertyName);
            return property?.propertyType == SerializedPropertyType.Boolean && property.boolValue;
        }

        private static int ReadArraySize(UnityEngine.Object target, string propertyName)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty? property = serialized.FindProperty(propertyName);
            return property != null && property.isArray ? property.arraySize : 0;
        }

        private static string ReadModuleName(SerializedProperty moduleProperty, int index)
        {
            string explicitName = ReadRelativeString(moduleProperty, "Name");
            if (!string.IsNullOrEmpty(explicitName))
                return explicitName;

            explicitName = ReadRelativeString(moduleProperty, "ModuleName");
            if (!string.IsNullOrEmpty(explicitName))
                return explicitName;

            return $"Module {index}";
        }

        private static List<VrseConfigExperienceInfo> ReadExperiences(SerializedProperty moduleProperty)
        {
            var experiences = new List<VrseConfigExperienceInfo>();
            SerializedProperty? list = moduleProperty.FindPropertyRelative("ExperienceDataList");
            if (list == null || !list.isArray)
                return experiences;

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty experienceProperty = list.GetArrayElementAtIndex(i);
                string devScene = ReadRelativeString(experienceProperty, "DevScene");
                string artScene = ReadRelativeString(experienceProperty, "ArtScene");
                experiences.Add(new VrseConfigExperienceInfo
                {
                    Index = i,
                    ExperienceId = ReadRelativeString(experienceProperty, "ExperienceId"),
                    Name = ReadRelativeString(experienceProperty, "Name"),
                    Type = ReadRelativeEnumOrString(experienceProperty, "Type"),
                    DevScene = devScene,
                    ArtScene = artScene,
                    StoryJsonPath = ReadRelativeString(experienceProperty, "StoryJsonPath"),
                    HasDevScene = !string.IsNullOrEmpty(devScene) && File.Exists(devScene),
                    HasArtScene = !string.IsNullOrEmpty(artScene) && File.Exists(artScene)
                });
            }

            return experiences;
        }

        private static string ReadRelativeString(SerializedProperty parent, string propertyName)
        {
            SerializedProperty? property = parent.FindPropertyRelative(propertyName);
            return property?.propertyType == SerializedPropertyType.String ? property.stringValue : string.Empty;
        }

        private static bool ReadRelativeBool(SerializedProperty parent, string propertyName)
        {
            SerializedProperty? property = parent.FindPropertyRelative(propertyName);
            return property?.propertyType == SerializedPropertyType.Boolean && property.boolValue;
        }

        private static string ReadRelativeEnumOrString(SerializedProperty parent, string propertyName)
        {
            SerializedProperty? property = parent.FindPropertyRelative(propertyName);
            if (property == null)
                return string.Empty;

            return property.propertyType switch
            {
                SerializedPropertyType.Enum => property.enumDisplayNames.Length > property.enumValueIndex ? property.enumDisplayNames[property.enumValueIndex] : property.enumValueIndex.ToString(),
                SerializedPropertyType.String => property.stringValue,
                _ => string.Empty
            };
        }
    }
}
