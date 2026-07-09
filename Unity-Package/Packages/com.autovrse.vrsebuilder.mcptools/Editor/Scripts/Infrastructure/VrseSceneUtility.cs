/*
┌──────────────────────────────────────────────────────────────────┐
│  VRseBuilder AI Tools                                             │
│  Unity-MCP extension tools for VRseBuilder workflows.             │
└──────────────────────────────────────────────────────────────────┘
*/

#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.autovrse.vrsebuilder.mcptools.Editor.Infrastructure
{
    internal static class VrseSceneUtility
    {
        public static string GetPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform current = gameObject.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        public static GameObject? FindGameObject(string? pathOrName)
        {
            if (string.IsNullOrWhiteSpace(pathOrName))
                return null;

            string query = pathOrName.Trim('/');
            GameObject active = GameObject.Find(query);
            if (active != null)
                return active;

            foreach (GameObject root in LoadedSceneRoots())
            {
                if (root.name == query)
                    return root;

                Transform? child = FindChildByPath(root.transform, query);
                if (child != null)
                    return child.gameObject;
            }

            string lower = query.ToLowerInvariant();
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.scene.IsValid() && go.name.ToLowerInvariant().Contains(lower));
        }

        public static IEnumerable<GameObject> LoadedSceneRoots()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                foreach (GameObject root in scene.GetRootGameObjects())
                    yield return root;
            }
        }

        private static Transform? FindChildByPath(Transform root, string path)
        {
            string[] segments = path.Split('/');
            int start = segments.Length > 0 && segments[0] == root.name ? 1 : 0;
            Transform current = root;
            for (int i = start; i < segments.Length; i++)
            {
                Transform? next = null;
                for (int c = 0; c < current.childCount; c++)
                {
                    Transform child = current.GetChild(c);
                    if (child.name == segments[i])
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                    return null;
                current = next;
            }

            return current;
        }
    }
}
