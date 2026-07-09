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
using UnityEngine;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Tools.Spatial
{
    public partial class Tool_VRseSpatial
    {
        [AiTool("vrse-spatial-analyze-scene", Title = "VRseBuilder / Spatial / Analyze Scene")]
        [Description("Analyze scene mesh candidates and broad surface placement data for no-marker spatial setup.")]
        public Dictionary<string, object> AnalyzeScene(string? filterTag = null, float filterRadius = -1f, string? center = null, bool includeSurfaces = true, int gridResolution = 10)
        {
            return MainThread.Instance.Run(() =>
            {
                Vector3 filterCenter = VrseSpatialUtility.ParseV3(center);
                bool useRadius = filterRadius > 0f && !string.IsNullOrWhiteSpace(center);
                var processedRoots = new HashSet<int>();
                var objects = new List<object>();
                Bounds sceneBounds = default;
                bool sceneBoundsInitialized = false;

                foreach (Renderer renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                {
                    if (renderer is ParticleSystemRenderer || renderer is TrailRenderer)
                        continue;

                    GameObject candidate = ResolveAnalysisRoot(renderer.gameObject);
                    if (!processedRoots.Add(candidate.GetInstanceID()))
                        continue;
                    if (!string.IsNullOrEmpty(filterTag) && !candidate.CompareTag(filterTag))
                        continue;
                    if (!VrseSpatialUtility.TryGetWorldBounds(candidate, includeChildren: true, out Bounds bounds))
                        continue;
                    if (useRadius && Vector3.Distance(bounds.center, filterCenter) > filterRadius)
                        continue;

                    if (!sceneBoundsInitialized)
                    {
                        sceneBounds = bounds;
                        sceneBoundsInitialized = true;
                    }
                    else
                    {
                        sceneBounds.Encapsulate(bounds);
                    }

                    objects.Add(new Dictionary<string, object>
                    {
                        { "name", candidate.name },
                        { "path", VrseSceneUtility.GetPath(candidate) },
                        { "classification", Classify(candidate, bounds) },
                        { "worldPosition", VrseSpatialUtility.V3(candidate.transform.position) },
                        { "worldRotation", VrseSpatialUtility.V3(candidate.transform.eulerAngles) },
                        { "bounds", VrseSpatialUtility.BoundsPayload(bounds) },
                        { "surfaces", VrseSpatialUtility.Surfaces(bounds) },
                        { "isStatic", candidate.isStatic },
                        { "hasCollider", candidate.GetComponentInChildren<Collider>() != null }
                    });
                }

                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "objectCount", objects.Count },
                    { "sceneBounds", sceneBoundsInitialized ? new Dictionary<string, object> { { "min", VrseSpatialUtility.V3(sceneBounds.min) }, { "max", VrseSpatialUtility.V3(sceneBounds.max) } } : null! },
                    { "objects", objects },
                    { "discoveredSurfaces", includeSurfaces && sceneBoundsInitialized ? DiscoverHorizontalSurfaces(sceneBounds, Mathf.Clamp(gridResolution, 2, 50)) : new List<object>() }
                };
            });
        }

        [AiTool("vrse-spatial-get-bounds", Title = "VRseBuilder / Spatial / Get Bounds")]
        [Description("Get world bounds and surface centers for a GameObject.")]
        public Dictionary<string, object> GetBounds(string gameObjectPath, bool includeChildren = true)
        {
            if (string.IsNullOrWhiteSpace(gameObjectPath))
                throw new ArgumentException("gameObjectPath is required.", nameof(gameObjectPath));

            return MainThread.Instance.Run(() =>
            {
                GameObject gameObject = VrseSceneUtility.FindGameObject(gameObjectPath)
                    ?? throw new InvalidOperationException($"GameObject '{gameObjectPath}' was not found.");
                if (!VrseSpatialUtility.TryGetWorldBounds(gameObject, includeChildren, out Bounds bounds))
                    throw new InvalidOperationException($"No Renderer or Collider found on '{gameObjectPath}'.");

                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "name", gameObject.name },
                    { "path", VrseSceneUtility.GetPath(gameObject) },
                    { "worldPosition", VrseSpatialUtility.V3(gameObject.transform.position) },
                    { "worldRotation", VrseSpatialUtility.V3(gameObject.transform.eulerAngles) },
                    { "bounds", VrseSpatialUtility.BoundsPayload(bounds) },
                    { "surfaces", VrseSpatialUtility.Surfaces(bounds) }
                };
            });
        }

        [AiTool("vrse-spatial-get-surface", Title = "VRseBuilder / Spatial / Get Surface")]
        [Description("Find a mesh surface on a target object by ray direction, with AABB fallback.")]
        public Dictionary<string, object> GetSurface(string targetObject, string direction = "0,-1,0", float offset = 1f)
        {
            if (string.IsNullOrWhiteSpace(targetObject))
                throw new ArgumentException("targetObject is required.", nameof(targetObject));

            return MainThread.Instance.Run(() =>
            {
                GameObject gameObject = VrseSceneUtility.FindGameObject(targetObject)
                    ?? throw new InvalidOperationException($"targetObject '{targetObject}' was not found.");
                if (!VrseSpatialUtility.TryGetWorldBounds(gameObject, true, out Bounds bounds))
                    throw new InvalidOperationException($"No Renderer or Collider found on '{targetObject}'.");

                Vector3 rayDirection = VrseSpatialUtility.ParseV3(direction).normalized;
                if (rayDirection == Vector3.zero)
                    rayDirection = Vector3.down;
                Vector3 rayOrigin = bounds.center - rayDirection * (bounds.extents.magnitude + offset);
                var temp = VrseSpatialUtility.AddTemporaryMeshColliders(gameObject);
                try
                {
                    if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, bounds.extents.magnitude * 2f + offset * 2f) && hit.collider.transform.IsChildOf(gameObject.transform))
                    {
                        return new Dictionary<string, object>
                        {
                            { "success", true },
                            { "surfacePoint", VrseSpatialUtility.V3(hit.point) },
                            { "surfaceNormal", VrseSpatialUtility.V3(hit.normal) },
                            { "objectName", hit.collider.gameObject.name },
                            { "objectPath", VrseSceneUtility.GetPath(hit.collider.gameObject) }
                        };
                    }
                }
                finally
                {
                    VrseSpatialUtility.RemoveTemporaryColliders(temp);
                }

                return BuildBoundsFaceFallback(gameObject, bounds, rayDirection);
            });
        }

        [AiTool("vrse-spatial-check-placement", Title = "VRseBuilder / Spatial / Check Placement")]
        [Description("Check whether a GameObject is floating, below floor, or overlapping nearby colliders.")]
        public Dictionary<string, object> CheckPlacement(string gameObjectPath, bool checkSurface = true, bool checkOverlap = true, bool checkFloor = true)
        {
            if (string.IsNullOrWhiteSpace(gameObjectPath))
                throw new ArgumentException("gameObjectPath is required.", nameof(gameObjectPath));

            return MainThread.Instance.Run(() =>
            {
                GameObject gameObject = VrseSceneUtility.FindGameObject(gameObjectPath)
                    ?? throw new InvalidOperationException($"GameObject '{gameObjectPath}' was not found.");
                if (!VrseSpatialUtility.TryGetWorldBounds(gameObject, true, out Bounds bounds))
                    throw new InvalidOperationException($"No Renderer or Collider found on '{gameObjectPath}'.");

                var issues = new List<string>();
                var result = new Dictionary<string, object>
                {
                    { "success", true },
                    { "name", gameObject.name },
                    { "worldPosition", VrseSpatialUtility.V3(gameObject.transform.position) },
                    { "boundsMin", VrseSpatialUtility.V3(bounds.min) },
                    { "boundsMax", VrseSpatialUtility.V3(bounds.max) }
                };

                if (checkSurface)
                    CheckSurfaceBelow(gameObject, bounds, result, issues);
                if (checkFloor && bounds.min.y < -0.05f)
                {
                    result["belowFloor"] = true;
                    issues.Add($"Object penetrates floor plane (bounds.min.y = {bounds.min.y:F2})");
                }
                else if (checkFloor)
                {
                    result["belowFloor"] = false;
                }
                if (checkOverlap)
                    CheckOverlaps(gameObject, bounds, result, issues);

                result["issues"] = issues;
                result["placementOk"] = issues.Count == 0;
                return result;
            });
        }

        [AiTool("vrse-spatial-list-probe-surfaces", Title = "VRseBuilder / Spatial / List Probe Surfaces")]
        [Description("Probe approximate horizontal/vertical surfaces on a target object.")]
        public Dictionary<string, object> ListProbeSurfaces(string targetObject, int probeResolution = 5, string axis = "vertical", float clusterTolerance = 0.05f)
        {
            if (string.IsNullOrWhiteSpace(targetObject))
                throw new ArgumentException("targetObject is required.", nameof(targetObject));

            return MainThread.Instance.Run(() =>
            {
                GameObject gameObject = VrseSceneUtility.FindGameObject(targetObject)
                    ?? throw new InvalidOperationException($"targetObject '{targetObject}' was not found.");
                if (!VrseSpatialUtility.TryGetWorldBounds(gameObject, true, out Bounds bounds))
                    throw new InvalidOperationException($"No Renderer or Collider found on '{targetObject}'.");

                probeResolution = Mathf.Clamp(probeResolution, 2, 20);
                var temp = VrseSpatialUtility.AddTemporaryMeshColliders(gameObject);
                try
                {
                    List<object> surfaces = ProbeSurfaces(gameObject, bounds, probeResolution, axis, clusterTolerance);
                    return new Dictionary<string, object>
                    {
                        { "success", true },
                        { "targetName", gameObject.name },
                        { "targetPath", VrseSceneUtility.GetPath(gameObject) },
                        { "targetBounds", VrseSpatialUtility.BoundsPayload(bounds) },
                        { "axis", axis },
                        { "surfaceCount", surfaces.Count },
                        { "surfaces", surfaces }
                    };
                }
                finally
                {
                    VrseSpatialUtility.RemoveTemporaryColliders(temp);
                }
            });
        }

        private static GameObject ResolveAnalysisRoot(GameObject gameObject)
        {
            Transform root = gameObject.transform;
            while (root.parent != null && root.parent.parent != null)
            {
                if (root.parent.GetComponent<Renderer>() == null && root.parent.childCount > 5)
                    break;
                root = root.parent;
            }

            return root.gameObject;
        }

        private static string Classify(GameObject gameObject, Bounds bounds)
        {
            bool hasManyChildren = gameObject.transform.childCount >= 3;
            bool isFlat = bounds.size.y < 0.15f && (bounds.size.x > 0.3f || bounds.size.z > 0.3f);
            bool isPanel = (bounds.size.x < 0.15f || bounds.size.z < 0.15f) && bounds.size.y > 0.3f;
            if (hasManyChildren) return "mechanism";
            if (isFlat) return "surface";
            if (isPanel) return "panel";
            return "object";
        }

        private static List<object> DiscoverHorizontalSurfaces(Bounds sceneBounds, int gridResolution)
        {
            var discovered = new List<object>();
            var temp = VrseSpatialUtility.AddTemporaryMeshColliders();
            try
            {
                float yTop = sceneBounds.max.y + 2f;
                for (int ix = 0; ix < gridResolution; ix++)
                for (int iz = 0; iz < gridResolution; iz++)
                {
                    Vector3 origin = new Vector3(
                        Mathf.Lerp(sceneBounds.min.x, sceneBounds.max.x, (ix + 0.5f) / gridResolution),
                        yTop,
                        Mathf.Lerp(sceneBounds.min.z, sceneBounds.max.z, (iz + 0.5f) / gridResolution));
                    if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, yTop - sceneBounds.min.y + 5f) && hit.normal.y > 0.7f)
                    {
                        discovered.Add(new Dictionary<string, object>
                        {
                            { "type", "horizontal" },
                            { "point", VrseSpatialUtility.V3(hit.point) },
                            { "normal", VrseSpatialUtility.V3(hit.normal) },
                            { "objectName", hit.collider.gameObject.name },
                            { "objectPath", VrseSceneUtility.GetPath(hit.collider.gameObject) }
                        });
                    }
                }
            }
            finally
            {
                VrseSpatialUtility.RemoveTemporaryColliders(temp);
            }

            return discovered
                .Cast<Dictionary<string, object>>()
                .GroupBy(item => item["objectPath"].ToString())
                .Select(group => (object)group.First())
                .ToList();
        }

        private static Dictionary<string, object> BuildBoundsFaceFallback(GameObject gameObject, Bounds bounds, Vector3 direction)
        {
            Vector3 point = bounds.center;
            Vector3 normal = -direction;
            if (direction.y < -0.5f) { point.y = bounds.max.y; normal = Vector3.up; }
            else if (direction.y > 0.5f) { point.y = bounds.min.y; normal = Vector3.down; }
            else if (direction.z > 0.5f) { point.z = bounds.min.z; normal = Vector3.back; }
            else if (direction.z < -0.5f) { point.z = bounds.max.z; normal = Vector3.forward; }
            else if (direction.x > 0.5f) { point.x = bounds.min.x; normal = Vector3.left; }
            else if (direction.x < -0.5f) { point.x = bounds.max.x; normal = Vector3.right; }

            return new Dictionary<string, object>
            {
                { "success", true },
                { "surfacePoint", VrseSpatialUtility.V3(point) },
                { "surfaceNormal", VrseSpatialUtility.V3(normal) },
                { "objectName", gameObject.name },
                { "objectPath", VrseSceneUtility.GetPath(gameObject) },
                { "fallback", true },
                { "message", "Raycast missed target mesh; returned AABB face center fallback." }
            };
        }

        private static void CheckSurfaceBelow(GameObject gameObject, Bounds bounds, Dictionary<string, object> result, List<string> issues)
        {
            var temp = VrseSpatialUtility.AddTemporaryMeshColliders();
            try
            {
                Vector3 origin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f) && !hit.collider.transform.IsChildOf(gameObject.transform))
                {
                    result["onSurface"] = true;
                    result["surfaceBelow"] = hit.collider.gameObject.name;
                    result["distanceToSurface"] = hit.distance;
                    if (hit.distance > 0.1f)
                        issues.Add($"Floating {hit.distance:F2}m above '{hit.collider.gameObject.name}'");
                }
                else
                {
                    result["onSurface"] = false;
                    result["surfaceBelow"] = "none";
                    issues.Add("No surface detected below object within 10m");
                }
            }
            finally
            {
                VrseSpatialUtility.RemoveTemporaryColliders(temp);
            }
        }

        private static void CheckOverlaps(GameObject gameObject, Bounds bounds, Dictionary<string, object> result, List<string> issues)
        {
            var overlapping = new List<string>();
            foreach (Collider collider in Physics.OverlapSphere(bounds.center, bounds.extents.magnitude))
            {
                if (collider.gameObject == gameObject || collider.transform.IsChildOf(gameObject.transform) || gameObject.transform.IsChildOf(collider.transform))
                    continue;
                if (bounds.Intersects(collider.bounds))
                    overlapping.Add(collider.gameObject.name);
            }

            result["overlapping"] = overlapping.Count > 0;
            if (overlapping.Count > 0)
            {
                result["overlappingObjects"] = overlapping;
                issues.Add($"Overlapping with: {string.Join(", ", overlapping)}");
            }
        }

        private static List<object> ProbeSurfaces(GameObject gameObject, Bounds bounds, int resolution, string axis, float tolerance)
        {
            var surfaces = new List<object>();
            if (axis == "vertical" || axis == "v" || axis == "all")
                surfaces.AddRange(ProbeVertical(gameObject, bounds, resolution, tolerance));
            if (axis == "horizontal_x" || axis == "hx" || axis == "all")
                surfaces.AddRange(ProbeHorizontal(gameObject, bounds, resolution, Vector3.right, "vertical_x", tolerance));
            if (axis == "horizontal_z" || axis == "hz" || axis == "all")
                surfaces.AddRange(ProbeHorizontal(gameObject, bounds, resolution, Vector3.forward, "vertical_z", tolerance));
            return surfaces;
        }

        private static IEnumerable<object> ProbeVertical(GameObject gameObject, Bounds bounds, int resolution, float tolerance)
        {
            var hits = new List<RaycastHit>();
            for (int ix = 0; ix < resolution; ix++)
            for (int iz = 0; iz < resolution; iz++)
            {
                Vector3 origin = new Vector3(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, (ix + 0.5f) / resolution),
                    bounds.max.y + 0.02f,
                    Mathf.Lerp(bounds.min.z, bounds.max.z, (iz + 0.5f) / resolution));
                hits.AddRange(Physics.RaycastAll(origin, Vector3.down, bounds.size.y + 0.04f).Where(hit => hit.collider.transform.IsChildOf(gameObject.transform) && hit.normal.y > 0.5f));
            }

            return ClusterHits(hits, "horizontal", hit => hit.point.y, tolerance);
        }

        private static IEnumerable<object> ProbeHorizontal(GameObject gameObject, Bounds bounds, int resolution, Vector3 direction, string orientation, float tolerance)
        {
            var hits = new List<RaycastHit>();
            bool sweepX = Mathf.Abs(direction.x) > 0.5f;
            for (int ia = 0; ia < resolution; ia++)
            for (int ib = 0; ib < resolution; ib++)
            {
                Vector3 origin = sweepX
                    ? new Vector3(bounds.min.x - 0.02f, Mathf.Lerp(bounds.min.y, bounds.max.y, (ia + 0.5f) / resolution), Mathf.Lerp(bounds.min.z, bounds.max.z, (ib + 0.5f) / resolution))
                    : new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, (ia + 0.5f) / resolution), Mathf.Lerp(bounds.min.y, bounds.max.y, (ib + 0.5f) / resolution), bounds.min.z - 0.02f);
                hits.AddRange(Physics.RaycastAll(origin, direction, sweepX ? bounds.size.x + 0.04f : bounds.size.z + 0.04f).Where(hit => hit.collider.transform.IsChildOf(gameObject.transform)));
            }

            return ClusterHits(hits, orientation, hit => sweepX ? hit.point.x : hit.point.z, tolerance);
        }

        private static IEnumerable<object> ClusterHits(List<RaycastHit> hits, string orientation, Func<RaycastHit, float> axisValue, float tolerance)
        {
            if (hits.Count == 0)
                return Array.Empty<object>();

            hits.Sort((a, b) => axisValue(a).CompareTo(axisValue(b)));
            var clusters = new List<List<RaycastHit>>();
            foreach (RaycastHit hit in hits)
            {
                List<RaycastHit>? cluster = clusters.FirstOrDefault(current => Mathf.Abs(current.Average(axisValue) - axisValue(hit)) <= tolerance);
                if (cluster == null)
                    clusters.Add(new List<RaycastHit> { hit });
                else
                    cluster.Add(hit);
            }

            return clusters.Select(cluster =>
            {
                Vector3 center = new Vector3(cluster.Average(hit => hit.point.x), cluster.Average(hit => hit.point.y), cluster.Average(hit => hit.point.z));
                Vector3 normal = cluster.Aggregate(Vector3.zero, (sum, hit) => sum + hit.normal).normalized;
                return (object)new Dictionary<string, object>
                {
                    { "orientation", orientation },
                    { "center", VrseSpatialUtility.V3(center) },
                    { "normal", VrseSpatialUtility.V3(normal) },
                    { "hitCount", cluster.Count },
                    { "meshName", cluster.GroupBy(hit => hit.collider.gameObject.name).OrderByDescending(group => group.Count()).First().Key }
                };
            }).ToList();
        }
    }
}
