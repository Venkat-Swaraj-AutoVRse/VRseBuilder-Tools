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
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Interactables
{
    public partial class Tool_VRseInteractables
    {
        [AiTool("vrse-interactable-convert", Title = "VRseBuilder / Interactable / Convert")]
        [Description("Convert a target GameObject using VRseBuilder MetaXR interactable conversion methods.")]
        public VrseInteractableConvertResult Convert
        (
            [Description("Conversion method: ConvertToVRseObject, ConvertToTouchObject, ConvertToGrabbable, ConvertToPlacePoint, CreatePlacePoint, or ConvertToRayInteractable.")]
            string methodName,
            [Description("Optional object name hint when objectPath is not provided.")]
            string? targetHint = null,
            [Description("Optional hierarchy path of the target object.")]
            string? objectPath = null
        )
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("methodName is required.", nameof(methodName));

            return MainThread.Instance.Run(() =>
            {
                GameObject target = ResolveTarget(objectPath, targetHint)
                    ?? throw new InvalidOperationException($"Could not find target object. Hint: '{targetHint}', Path: '{objectPath}'.");
                Type converterType = VrseTypeResolver.FindType("VRseBuilder.Platform.MetaXR.Editor.MetaXRInteractableConverter")
                    ?? throw new InvalidOperationException("MetaXRInteractableConverter was not found. Ensure VRseBuilder MetaXR editor tools are installed and compiled.");

                object? rawResult = methodName.Trim() switch
                {
                    "ConvertToVRseObject" => VrseReflection.InvokeStatic(converterType, "ConvertToNetworkMetaXRBaseItem", target),
                    "ConvertToTouchObject" => InvokeVoid(converterType, "ConvertToTouchable", target),
                    "ConvertToGrabbable" => VrseReflection.InvokeStatic(converterType, "ConvertToNetworkMetaXRGrabbable", target),
                    "ConvertToPlacePoint" => VrseReflection.InvokeStatic(converterType, "ConvertToNetworkMetaXRPlacePoint", target),
                    "CreatePlacePoint" => InvokeVoid(converterType, "CreateNetworkMetaXRPlacePoint", target, null),
                    "ConvertToRayInteractable" => InvokeVoid(converterType, "ConvertToRayPointerInteractable", target),
                    _ => throw new ArgumentException($"Unknown conversion method: {methodName}", nameof(methodName))
                };

                if (rawResult is bool success && !success)
                    throw new InvalidOperationException($"Conversion {methodName} failed in VRseBuilder backend.");

                GameObject resolved = Selection.activeGameObject != null ? Selection.activeGameObject : target;
                return new VrseInteractableConvertResult
                {
                    Success = true,
                    ResolvedObject = VrseSceneUtility.GetPath(resolved),
                    Message = $"Successfully executed {methodName}."
                };
            });
        }

        private static GameObject? ResolveTarget(string? objectPath, string? targetHint)
        {
            return VrseSceneUtility.FindGameObject(objectPath)
                ?? VrseSceneUtility.FindGameObject(targetHint)
                ?? Selection.activeGameObject;
        }

        private static object InvokeVoid(Type type, string methodName, params object?[] args)
        {
            VrseReflection.InvokeStatic(type, methodName, args);
            return true;
        }
    }
}
