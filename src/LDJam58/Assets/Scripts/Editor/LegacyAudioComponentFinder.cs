using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LegacyAudioComponentFinder : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<ComponentInfo> _foundComponents = new List<ComponentInfo>();
    private bool _hasScanned = false;
    private bool _isScanning = false;
    private string _scanStatus = "";

    private static readonly string[] LegacyComponentTypes = new[]
    {
        "SceneBackgroundMusic",
        "IntroLoopSceneBackgroundMusic",
        "IntroLoopMusicPlaylist",
        "InitGameMusicPlayer",
        "InitIntroLoopAudioPlayer",
        "MixerVolumeSlider",
        "InitAudioVolumeLevel",
        "OnVolumeChangedSound"
    };

    [MenuItem("Tools/Audio/Legacy Audio Component Finder")]
    public static void ShowWindow()
    {
        var window = GetWindow<LegacyAudioComponentFinder>("Legacy Audio Finder");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Legacy Audio Component Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool finds legacy audio components that should be migrated to the new AudioSystem:\n" +
            "• SceneBackgroundMusic\n" +
            "• IntroLoopSceneBackgroundMusic\n" +
            "• IntroLoopMusicPlaylist\n" +
            "• InitGameMusicPlayer\n" +
            "• InitIntroLoopAudioPlayer\n" +
            "• MixerVolumeSlider\n" +
            "• InitAudioVolumeLevel\n" +
            "• OnVolumeChangedSound",
            MessageType.Info);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(_isScanning);
        if (GUILayout.Button("Scan All Scenes & Prefabs", GUILayout.Height(30)))
        {
            ScanAllAssets();
        }
        EditorGUI.EndDisabledGroup();

        if (_isScanning)
        {
            EditorGUILayout.HelpBox($"Scanning... {_scanStatus}", MessageType.Info);
        }

        EditorGUILayout.Space();

        if (_hasScanned)
        {
            EditorGUILayout.LabelField($"Found {_foundComponents.Count} legacy component(s):", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_foundComponents.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ No legacy audio components found! All scenes are clean.", MessageType.Info);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                for (var i = 0; i < _foundComponents.Count; i++)
                {
                    DrawComponentInfo(_foundComponents[i], i);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    private void DrawComponentInfo(ComponentInfo info, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{index + 1}. {info.ComponentType}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        
        var buttonText = info.IsPrefab ? "Select Prefab" : "Open Scene";
        if (GUILayout.Button(buttonText, GUILayout.Width(120)))
        {
            if (info.IsPrefab)
            {
                SelectPrefab(info);
            }
            else
            {
                OpenSceneAndHighlight(info);
            }
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Type:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  {(info.IsPrefab ? "Prefab" : "Scene")}", EditorStyles.wordWrappedLabel);

        EditorGUILayout.LabelField(info.IsPrefab ? "Prefab Path:" : "Scene Path:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  {info.AssetPath}", EditorStyles.wordWrappedLabel);

        EditorGUILayout.LabelField("Object Path:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  {info.ObjectPath}", EditorStyles.wordWrappedLabel);

        if (!string.IsNullOrEmpty(info.HierarchyPath))
        {
            EditorGUILayout.LabelField("Component Hierarchy:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"  {info.HierarchyPath}", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void ScanAllAssets()
    {
        _foundComponents.Clear();
        _hasScanned = true;
        _isScanning = true;

        try
        {
            var originalScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            // Scan all scenes
            var scenePaths = GetAllScenePaths();
            _scanStatus = $"Scanning {scenePaths.Count} scenes...";
            Repaint();

            foreach (var scenePath in scenePaths)
            {
                _scanStatus = $"Scanning scene: {Path.GetFileName(scenePath)}";
                Repaint();
                ScanScene(scenePath);
            }

            // Restore original scene
            if (!string.IsNullOrEmpty(originalScene.path))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalScene.path);
            }

            // Scan all prefabs
            var prefabPaths = GetAllPrefabPaths();
            _scanStatus = $"Scanning {prefabPaths.Count} prefabs...";
            Repaint();

            for (var i = 0; i < prefabPaths.Count; i++)
            {
                var prefabPath = prefabPaths[i];
                _scanStatus = $"Scanning prefab {i + 1}/{prefabPaths.Count}: {Path.GetFileName(prefabPath)}";
                Repaint();
                ScanPrefab(prefabPath);
            }

            _scanStatus = "Scan complete!";
        }
        finally
        {
            _isScanning = false;
            Repaint();
        }
    }

    private void ScanScene(string scenePath)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
        var sceneName = Path.GetFileNameWithoutExtension(scenePath);

        foreach (var componentTypeName in LegacyComponentTypes)
        {
            var componentType = GetComponentType(componentTypeName);
            if (componentType == null)
                continue;

            var allObjects = Object.FindObjectsOfType(componentType, true);
            foreach (var obj in allObjects)
            {
                if (obj is Component component)
                {
                    var info = new ComponentInfo
                    {
                        IsPrefab = false,
                        AssetPath = scenePath,
                        AssetName = sceneName,
                        ComponentType = componentTypeName,
                        GameObject = component.gameObject,
                        ObjectPath = GetGameObjectPath(component.gameObject),
                        HierarchyPath = GetHierarchyPath(component.gameObject)
                    };
                    _foundComponents.Add(info);
                }
            }
        }
    }

    private void ScanPrefab(string prefabPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            return;

        var prefabName = Path.GetFileNameWithoutExtension(prefabPath);

        foreach (var componentTypeName in LegacyComponentTypes)
        {
            var componentType = GetComponentType(componentTypeName);
            if (componentType == null)
                continue;

            var components = prefab.GetComponentsInChildren(componentType, true);
            foreach (var component in components)
            {
                if (component != null)
                {
                    var info = new ComponentInfo
                    {
                        IsPrefab = true,
                        AssetPath = prefabPath,
                        AssetName = prefabName,
                        ComponentType = componentTypeName,
                        GameObject = component.gameObject,
                        ObjectPath = GetGameObjectPath(component.gameObject),
                        HierarchyPath = GetHierarchyPath(component.gameObject)
                    };
                    _foundComponents.Add(info);
                }
            }
        }
    }

    private System.Type GetComponentType(string typeName)
    {
        var componentType = System.Type.GetType(typeName);
        if (componentType != null)
            return componentType;

        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            componentType = assembly.GetType(typeName);
            if (componentType != null)
                return componentType;
        }

        return null;
    }

    private string GetGameObjectPath(GameObject obj)
    {
        var path = obj.name;
        var parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private string GetHierarchyPath(GameObject obj)
    {
        var components = obj.GetComponents<Component>();
        var componentNames = components
            .Where(c => c != null)
            .Select(c => c.GetType().Name)
            .ToArray();
        return string.Join(" → ", componentNames);
    }

    private void OpenSceneAndHighlight(ComponentInfo info)
    {
        if (string.IsNullOrEmpty(info.AssetPath) || info.IsPrefab)
            return;

        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(info.AssetPath);
        Selection.activeGameObject = info.GameObject;
        EditorGUIUtility.PingObject(info.GameObject);
    }

    private void SelectPrefab(ComponentInfo info)
    {
        if (string.IsNullOrEmpty(info.AssetPath) || !info.IsPrefab)
            return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.AssetPath);
        if (prefab != null)
        {
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            // Try to find the specific GameObject in the prefab
            if (info.GameObject != null)
            {
                var prefabInstance = PrefabUtility.LoadPrefabContents(info.AssetPath);
                var targetObj = FindGameObjectInPrefab(prefabInstance, info.ObjectPath);
                if (targetObj != null)
                {
                    Selection.activeGameObject = targetObj;
                }
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }
    }

    private GameObject FindGameObjectInPrefab(GameObject root, string path)
    {
        var parts = path.Split('/');
        var current = root;

        for (var i = 0; i < parts.Length; i++)
        {
            if (current == null)
                return null;

            if (i == parts.Length - 1)
            {
                if (current.name == parts[i])
                    return current;
            }
            else
            {
                var found = false;
                foreach (Transform child in current.transform)
                {
                    if (child.name == parts[i])
                    {
                        current = child.gameObject;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return null;
            }
        }

        return current;
    }

    private List<string> GetAllScenePaths()
    {
        var paths = new List<string>();
        var guids = AssetDatabase.FindAssets("t:Scene");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                continue;

            // Exclude built-in "UnityEditor" or "Package" (Packages/...) scenes and read-only scenes
            if (IsExcludedPath(path))
                continue;

            // Exclude read-only scenes (cannot edit)
            if (AssetDatabase.IsOpenForEdit(path) == false)
                continue;

            paths.Add(path);
        }

        return paths;
    }

    private List<string> GetAllPrefabPaths()
    {
        var paths = new List<string>();
        var guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                continue;

            // Exclude built-in "UnityEditor" or "Package" (Packages/...) prefabs and read-only prefabs
            if (IsExcludedPath(path))
                continue;

            // Exclude read-only assets (cannot edit)
            if (AssetDatabase.IsOpenForEdit(path) == false)
                continue;

            paths.Add(path);
        }

        return paths;
    }

    private bool IsExcludedPath(string path)
    {
        // Excludes: built-in unity editor folders, Packages, Library, ProjectSettings, and anything outside "Assets/"
        // Only Assets/ is editable/project content.
        if (!path.StartsWith("Assets/"))
            return true;

        // Exclude typical system folders (customize as needed)
        if (path.StartsWith("Assets/Gizmos/") ||
            path.StartsWith("Assets/Editor Default Resources/") ||
            path.StartsWith("Assets/Plugins/") && (path.Contains("/UnityEditor/") || path.Contains("/Editor/") || path.Contains("/Runtime/Editor/")) ||
            path.StartsWith("Assets/Editor/") // You may want to allow Editor prefabs/scripts, but not built-in ones
            )
        {
            return true;
        }

        // Could add more rules here if you have special folders

        return false;
    }

    private class ComponentInfo
    {
        public bool IsPrefab;
        public string AssetPath;
        public string AssetName;
        public string ComponentType;
        public GameObject GameObject;
        public string ObjectPath;
        public string HierarchyPath;
    }
}
