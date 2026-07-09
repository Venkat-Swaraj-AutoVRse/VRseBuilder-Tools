/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Reflection;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Reflection
{
    internal static class VrseReflection
    {
        public static object? InvokeStatic(Type? type, string methodName)
        {
            if (type == null)
                return null;

            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return method?.Invoke(null, null);
        }

        public static string InvokeStaticString(Type? type, string methodName)
        {
            return InvokeStatic(type, methodName)?.ToString() ?? string.Empty;
        }

        public static bool InvokeStaticBool(Type? type, string methodName)
        {
            object? value = InvokeStatic(type, methodName);
            return value is bool boolValue && boolValue;
        }
    }
}
