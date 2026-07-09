/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System;
using System.Linq;
using System.Reflection;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Reflection
{
    internal static class VrseTypeResolver
    {
        public static Type? FindType(string fullNameOrName)
        {
            if (string.IsNullOrWhiteSpace(fullNameOrName))
                return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? exact = assembly.GetType(fullNameOrName, throwOnError: false);
                if (exact != null)
                    return exact;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(type => type != null).Cast<Type>().ToArray();
                }

                Type? byName = types.FirstOrDefault(type =>
                    string.Equals(type.FullName, fullNameOrName, StringComparison.Ordinal) ||
                    string.Equals(type.Name, fullNameOrName, StringComparison.Ordinal));
                if (byName != null)
                    return byName;
            }

            return null;
        }
    }
}
