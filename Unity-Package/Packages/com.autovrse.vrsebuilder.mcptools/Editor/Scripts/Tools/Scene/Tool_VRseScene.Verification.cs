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
using System.Linq;
using com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Scene
{
    public partial class Tool_VRseScene
    {
        [AiTool("vrse-scene-list-loaded", Title = "VRseBuilder / Scene / List Loaded Objects")]
        [Description("Search GameObjects across all loaded scenes, including inactive objects.")]
        public Dictionary<string, object> ListLoadedScenes(string namePattern, bool includeInactive = true, bool regex = false, int limit = 500, string? sceneName = null)
        {
            if (string.IsNullOrWhiteSpace(namePattern))
                throw new ArgumentException("namePattern is required.", nameof(namePattern));

            return MainThread.Instance.Run(() =>
            {
                System.Text.RegularExpressions.Regex? compiled = null;
                if (regex)
                    compiled = new System.Text.RegularExpressions.Regex(namePattern);

                string lowerPattern = namePattern.ToLowerInvariant();
                var matches = new List<object>();
                int totalFound = 0;
                foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (gameObject == null || !gameObject.scene.IsValid())
                        continue;
                    if (!includeInactive && !gameObject.activeInHierarchy)
                        continue;
                    if (!string.IsNullOrEmpty(sceneName) && gameObject.scene.name != sceneName)
                        continue;
                    bool isMatch = regex ? compiled!.IsMatch(gameObject.name) : gameObject.name.ToLowerInvariant().Contains(lowerPattern);
                    if (!isMatch)
                        continue;

                    totalFound++;
                    if (matches.Count >= limit)
                        continue;
                    matches.Add(new Dictionary<string, object>
                    {
                        { "name", gameObject.name },
                        { "path", VrseSceneUtility.GetPath(gameObject) },
                        { "instanceId", gameObject.GetInstanceID() },
                        { "sceneName", gameObject.scene.name },
                        { "active", gameObject.activeInHierarchy },
                        { "tag", gameObject.tag },
                        { "layer", LayerMask.LayerToName(gameObject.layer) }
                    });
                }

                var scenes = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    scenes.Add(SceneManager.GetSceneAt(i).name);

                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "namePattern", namePattern },
                    { "totalFound", totalFound },
                    { "returned", matches.Count },
                    { "limit", limit },
                    { "matches", matches },
                    { "scenesSearched", scenes },
                    { "truncated", totalFound > matches.Count }
                };
            });
        }

        [AiTool("vrse-scene-get-components", Title = "VRseBuilder / Scene / Get Components")]
        [Description("Return all components and serialized properties on a GameObject path/name.")]
        public Dictionary<string, object> GetComponents(string gameObjectPath, bool includeProperties = true, string? componentType = null)
        {
            if (string.IsNullOrWhiteSpace(gameObjectPath))
                throw new ArgumentException("gameObjectPath is required.", nameof(gameObjectPath));

            return MainThread.Instance.Run(() =>
            {
                GameObject gameObject = VrseSceneUtility.FindGameObject(gameObjectPath)
                    ?? throw new InvalidOperationException($"GameObject '{gameObjectPath}' was not found.");
                var components = new List<object>();
                foreach (UnityEngine.Component component in gameObject.GetComponents<UnityEngine.Component>())
                {
                    if (component == null)
                    {
                        components.Add(new Dictionary<string, object> { { "type", "MISSING_SCRIPT" }, { "enabled", false } });
                        continue;
                    }

                    Type type = component.GetType();
                    if (!string.IsNullOrEmpty(componentType) && type.Name != componentType && type.FullName != componentType)
                        continue;

                    var entry = new Dictionary<string, object>
                    {
                        { "type", type.Name },
                        { "fullType", type.FullName ?? string.Empty },
                        { "enabled", component is Behaviour behaviour ? behaviour.enabled : true },
                        { "instanceId", component.GetInstanceID() }
                    };
                    if (includeProperties)
                        entry["properties"] = SerializeProperties(component);
                    components.Add(entry);
                }

                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "name", gameObject.name },
                    { "path", VrseSceneUtility.GetPath(gameObject) },
                    { "instanceId", gameObject.GetInstanceID() },
                    { "componentCount", components.Count },
                    { "components", components }
                };
            });
        }

        private static List<object> SerializeProperties(UnityEngine.Component component)
        {
            var properties = new List<object>();
            try
            {
                var serializedObject = new SerializedObject(component);
                SerializedProperty iterator = serializedObject.GetIterator();
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        properties.Add(new Dictionary<string, object>
                        {
                            { "name", iterator.name },
                            { "displayName", iterator.displayName },
                            { "type", iterator.propertyType.ToString() },
                            { "value", ReadSerializedValue(iterator) },
                            { "editable", iterator.editable }
                        });
                    } while (iterator.NextVisible(false));
                }
            }
            catch (Exception ex)
            {
                properties.Add(new Dictionary<string, object> { { "error", ex.Message } });
            }

            return properties;
        }

        private static object? ReadSerializedValue(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => property.intValue,
                SerializedPropertyType.Boolean => property.boolValue,
                SerializedPropertyType.Float => property.floatValue,
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.ObjectReference => property.objectReferenceValue != null ? new Dictionary<string, object> { { "name", property.objectReferenceValue.name }, { "type", property.objectReferenceValue.GetType().Name }, { "instanceId", property.objectReferenceValue.GetInstanceID() } } : null,
                SerializedPropertyType.Enum => property.enumValueIndex,
                SerializedPropertyType.Vector2 => new Dictionary<string, object> { { "x", property.vector2Value.x }, { "y", property.vector2Value.y } },
                SerializedPropertyType.Vector3 => new Dictionary<string, object> { { "x", property.vector3Value.x }, { "y", property.vector3Value.y }, { "z", property.vector3Value.z } },
                SerializedPropertyType.Color => new Dictionary<string, object> { { "r", property.colorValue.r }, { "g", property.colorValue.g }, { "b", property.colorValue.b }, { "a", property.colorValue.a } },
                _ => property.propertyType.ToString()
            };
        }
    }
}
