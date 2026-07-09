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
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Scene
{
    public partial class Tool_VRseScene
    {
        [AiTool("vrse-scene-query-objects-list", Title = "VRseBuilder / Scene / Query Objects List")]
        [Description("List QueryObjectsIdManager query objects and VRse-related components in loaded scenes.")]
        public VrseQueryObjectsListResult QueryObjectsList()
        {
            return MainThread.Instance.Run(() =>
            {
                List<object> queries = GetQueryObjects();
                var result = new VrseQueryObjectsListResult { Success = true };
                foreach (object query in queries)
                {
                    if (query is not Component component || component.gameObject == null)
                        continue;

                    result.QueryObjects.Add(new VrseQueryObjectInfo
                    {
                        QueryName = VrseReflection.GetString(query, "Name"),
                        Id = VrseReflection.GetInt(query, "ID"),
                        IsIdValid = VrseReflection.GetBool(query, "IsIDValid"),
                        GameObjectName = component.gameObject.name,
                        GameObjectPath = VrseSceneUtility.GetPath(component.gameObject),
                        ActiveInHierarchy = component.gameObject.activeInHierarchy,
                        VrseComponents = GetVrseComponentLabels(component.gameObject)
                    });
                }

                result.Count = result.QueryObjects.Count;
                return result;
            });
        }

        private static List<object> GetQueryObjects()
        {
            Type? managerType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.QueryObjectsIdManager");
            if (managerType != null)
            {
                object? manager = VrseReflection.FindSceneObjectsOfType(managerType).FirstOrDefault();
                object? fromManager = VrseReflection.InvokeInstance(manager, "GetAllGameObjectQueries");
                List<object> queryObjects = VrseReflection.AsObjectList(fromManager);
                if (queryObjects.Count > 0)
                    return queryObjects;
            }

            Type queryType = VrseTypeResolver.FindType("VRseBuilder.Core.Framework.GameObjectQuery")
                ?? throw new InvalidOperationException("GameObjectQuery type was not found. Ensure VRseBuilder SDK is installed and compiled.");
            return VrseReflection.FindSceneObjectsOfType(queryType);
        }

        private static List<string> GetVrseComponentLabels(GameObject gameObject)
        {
            var labels = new List<string>();
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                string typeName = component.GetType().Name;
                if (typeName.Contains("Grabbable")) labels.Add("Grabbable");
                else if (typeName.Contains("PlacePoint")) labels.Add("PlacePoint");
                else if (typeName.Contains("BaseItem")) labels.Add("BaseItem");
            }

            return labels.Distinct().ToList();
        }
    }
}
