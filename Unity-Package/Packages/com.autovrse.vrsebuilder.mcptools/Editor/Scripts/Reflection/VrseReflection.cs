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

        public static object? InvokeStatic(Type? type, string methodName, params object?[] args)
        {
            if (type == null)
                return null;

            MethodInfo? method = FindMethod(type, methodName, isStatic: true, args.Length);
            return method?.Invoke(null, args);
        }

        public static object? CreateInstance(Type? type)
        {
            return type == null ? null : Activator.CreateInstance(type);
        }

        public static object? InvokeInstance(object? instance, string methodName, params object?[] args)
        {
            if (instance == null)
                return null;

            MethodInfo? method = FindMethod(instance.GetType(), methodName, isStatic: false, args.Length);
            return method?.Invoke(instance, args);
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

        private static MethodInfo? FindMethod(Type type, string methodName, bool isStatic, int argCount)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            foreach (MethodInfo method in type.GetMethods(flags))
            {
                if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
                    continue;

                if (method.GetParameters().Length == argCount)
                    return method;
            }

            return null;
        }
    }
}
