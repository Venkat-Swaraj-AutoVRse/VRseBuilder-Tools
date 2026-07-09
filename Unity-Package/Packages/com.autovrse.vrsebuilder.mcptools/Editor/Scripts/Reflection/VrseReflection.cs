/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        public static object? GetMemberValue(object? instance, string memberName)
        {
            if (instance == null)
                return null;

            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
                return property.GetValue(instance);

            FieldInfo? field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(instance);
        }

        public static string GetString(object? instance, string memberName)
        {
            return GetMemberValue(instance, memberName)?.ToString() ?? string.Empty;
        }

        public static int GetInt(object? instance, string memberName)
        {
            object? value = GetMemberValue(instance, memberName);
            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return (int)longValue;

            return int.TryParse(value?.ToString(), out int parsed) ? parsed : 0;
        }

        public static bool GetBool(object? instance, string memberName)
        {
            object? value = GetMemberValue(instance, memberName);
            return value is bool boolValue && boolValue;
        }

        public static List<object> AsObjectList(object? value)
        {
            if (value == null)
                return new List<object>();

            if (value is string)
                return new List<object>();

            if (value is IEnumerable enumerable)
                return enumerable.Cast<object>().Where(item => item != null).ToList();

            return new List<object>();
        }

        public static List<object> FindSceneObjectsOfType(Type type)
        {
            return Resources.FindObjectsOfTypeAll(type)
                .Where(obj => obj is Component component && component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded)
                .Cast<object>()
                .ToList();
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
