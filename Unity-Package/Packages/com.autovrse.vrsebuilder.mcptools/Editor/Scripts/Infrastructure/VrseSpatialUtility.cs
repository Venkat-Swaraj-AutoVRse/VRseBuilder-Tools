/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseSpatialUtility
    {
        public static string V3(Vector3 value) => $"Vector3({value.x:F3},{value.y:F3},{value.z:F3})";

        public static Vector3 ParseV3(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Vector3.zero;

            string value = text.Trim();
            if (value.StartsWith("Vector3(") && value.EndsWith(")"))
                value = value.Substring(8, value.Length - 9);

            string[] parts = value.Split(',');
            if (parts.Length < 3)
                return Vector3.zero;

            float.TryParse(parts[0].Trim(), out float x);
            float.TryParse(parts[1].Trim(), out float y);
            float.TryParse(parts[2].Trim(), out float z);
            return new Vector3(x, y, z);
        }

        public static bool TryGetWorldBounds(GameObject gameObject, bool includeChildren, out Bounds bounds)
        {
            Renderer[] renderers = includeChildren
                ? gameObject.GetComponentsInChildren<Renderer>(true)
                : gameObject.GetComponents<Renderer>();

            bounds = default;
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer)
                    continue;

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!found)
            {
                Collider[] colliders = includeChildren
                    ? gameObject.GetComponentsInChildren<Collider>(true)
                    : gameObject.GetComponents<Collider>();

                foreach (Collider collider in colliders)
                {
                    if (!found)
                    {
                        bounds = collider.bounds;
                        found = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }

            return found;
        }

        public static Dictionary<string, object> BoundsPayload(Bounds bounds)
        {
            return new Dictionary<string, object>
            {
                { "center", V3(bounds.center) },
                { "size", V3(bounds.size) },
                { "min", V3(bounds.min) },
                { "max", V3(bounds.max) }
            };
        }

        public static Dictionary<string, object> Surfaces(Bounds bounds)
        {
            return new Dictionary<string, object>
            {
                { "top", V3(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z)) },
                { "bottom", V3(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)) },
                { "front", V3(new Vector3(bounds.center.x, bounds.center.y, bounds.max.z)) },
                { "back", V3(new Vector3(bounds.center.x, bounds.center.y, bounds.min.z)) },
                { "right", V3(new Vector3(bounds.max.x, bounds.center.y, bounds.center.z)) },
                { "left", V3(new Vector3(bounds.min.x, bounds.center.y, bounds.center.z)) }
            };
        }

        public static List<MeshCollider> AddTemporaryMeshColliders(GameObject? root = null)
        {
            var added = new List<MeshCollider>();
            MeshFilter[] filters = root != null ? root.GetComponentsInChildren<MeshFilter>(true) : Object.FindObjectsOfType<MeshFilter>();
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null)
                    continue;

                added.Add(filter.gameObject.AddComponent<MeshCollider>());
            }

            Physics.SyncTransforms();
            return added;
        }

        public static void RemoveTemporaryColliders(List<MeshCollider> colliders)
        {
            foreach (MeshCollider collider in colliders)
            {
                if (collider != null)
                    Object.DestroyImmediate(collider);
            }
        }
    }
}
