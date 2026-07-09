/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.ComponentModel;
using com.autovrse.vrsebuilder.mcptools.Editor.Models;
using com.autovrse.vrsebuilder.mcptools.Editor.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Interactables
{
    public partial class Tool_VRseRotator
    {
        [AiTool("vrse-rotator-analyze", Title = "VRseBuilder / Rotator / Analyze Mesh")]
        [Description("Analyze scene mesh candidates for creating a VRseBuilder pivot rotate limiter.")]
        public VrseRotatorAnalyzeResult Analyze([Description("GameObject name or fuzzy name to analyze.")] string gameObjectName)
        {
            if (string.IsNullOrWhiteSpace(gameObjectName))
                throw new ArgumentException("gameObjectName is required.", nameof(gameObjectName));

            return MainThread.Instance.Run(() =>
            {
                Type setupType = VrseTypeResolver.FindType("VRsePivotRotateLimiterSetup")
                    ?? throw new InvalidOperationException("VRsePivotRotateLimiterSetup was not found. Ensure the project editor utility is installed and compiled.");
                string json = VrseReflection.InvokeStatic(setupType, "Analyze", gameObjectName.Trim())?.ToString() ?? string.Empty;
                return new VrseRotatorAnalyzeResult { Success = !string.IsNullOrEmpty(json), Json = json };
            });
        }

        [AiTool("vrse-rotator-create", Title = "VRseBuilder / Rotator / Create")]
        [Description("Create a VRseBuilder pivot rotate limiter from analyzed mesh parameters.")]
        public VrseRotatorCreateResult Create(
            int instanceId = 0,
            string parentObjectPath = "",
            string rotatingMeshName = "",
            string staticMeshNames = "",
            bool rootMeshIsStatic = true,
            int rotationAxis = 1,
            string rotationKind = "",
            float minAngle = 0f,
            float maxAngle = 90f,
            bool useRootAsParent = false,
            float hingeDirX = 0f,
            float hingeDirY = 0f,
            float hingeDirZ = 0f,
            bool overridePivot = false,
            float pivotX = 0f,
            float pivotY = 0f,
            float pivotZ = 0f)
        {
            return MainThread.Instance.Run(() =>
            {
                Type setupType = VrseTypeResolver.FindType("VRsePivotRotateLimiterSetup")
                    ?? throw new InvalidOperationException("VRsePivotRotateLimiterSetup was not found. Ensure the project editor utility is installed and compiled.");
                Type paramsType = setupType.GetNestedType("Params")
                    ?? throw new InvalidOperationException("VRsePivotRotateLimiterSetup.Params was not found.");
                object parameters = Activator.CreateInstance(paramsType)
                    ?? throw new InvalidOperationException("Could not create VRsePivotRotateLimiterSetup.Params.");

                Set(paramsType, parameters, "instanceId", instanceId);
                Set(paramsType, parameters, "parentObjectPath", parentObjectPath ?? string.Empty);
                Set(paramsType, parameters, "rotatingMeshName", rotatingMeshName ?? string.Empty);
                Set(paramsType, parameters, "staticMeshNames", staticMeshNames ?? string.Empty);
                Set(paramsType, parameters, "rootMeshIsStatic", rootMeshIsStatic);
                Set(paramsType, parameters, "rotationAxis", rotationAxis);
                Set(paramsType, parameters, "rotationKind", rotationKind ?? string.Empty);
                Set(paramsType, parameters, "minAngle", minAngle);
                Set(paramsType, parameters, "maxAngle", maxAngle);
                Set(paramsType, parameters, "useRootAsParent", useRootAsParent);
                Set(paramsType, parameters, "hingeDirX", hingeDirX);
                Set(paramsType, parameters, "hingeDirY", hingeDirY);
                Set(paramsType, parameters, "hingeDirZ", hingeDirZ);
                Set(paramsType, parameters, "overridePivot", overridePivot);
                Set(paramsType, parameters, "pivotX", pivotX);
                Set(paramsType, parameters, "pivotY", pivotY);
                Set(paramsType, parameters, "pivotZ", pivotZ);

                string raw = VrseReflection.InvokeStatic(setupType, "Create", parameters)?.ToString() ?? string.Empty;
                string[] parts = raw.Split(new[] { "|||" }, StringSplitOptions.None);
                if (parts.Length > 0 && parts[0] == "FAIL")
                    return new VrseRotatorCreateResult { Success = false, Message = parts.Length > 1 ? parts[1] : "Unknown error", RawResult = raw };

                return new VrseRotatorCreateResult
                {
                    Success = true,
                    Message = parts.Length > 1 ? $"Created {parts[1]}" : "Created rotator.",
                    RootObjectName = parts.Length > 1 ? parts[1] : string.Empty,
                    InstanceId = parts.Length > 2 && int.TryParse(parts[2], out int id) ? id : 0,
                    RawResult = raw
                };
            });
        }

        private static void Set(Type type, object instance, string fieldName, object value)
        {
            type.GetField(fieldName)?.SetValue(instance, value);
        }
    }
}
