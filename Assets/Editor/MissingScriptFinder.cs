using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    private static void FindMissingScripts()
    {
        int goCount = 0, componentsCount = 0, missingCount = 0;
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            goCount++;
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    string path = GetFullPath(go);
                    Debug.LogError($"[MissingScriptFinder] Missing script on GameObject: '{path}' (component index {i})", go);
                }
            }
        }

        Debug.Log($"[MissingScriptFinder] Scan complete. GameObjects: {goCount}, Components: {componentsCount}, Missing: {missingCount}");
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}