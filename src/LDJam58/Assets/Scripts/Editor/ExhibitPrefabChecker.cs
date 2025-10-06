using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExhibitPrefabChecker : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<string> _missingPrefabs = new List<string>();
    private List<string> _extraPrefabs = new List<string>();
    private bool _showResults = false;
    private string _statusMessage = "";

    private const string ObjPrefabsFolder = "Assets/Prefabs/Exhibits";
    private const string ExhibitPrefabsFolder = "Assets/Resources/Exh/Prefabs";

    [MenuItem("Tools/QA/Check Missing Exhibit Prefabs")]
    public static void ShowWindow()
    {
        var window = GetWindow<ExhibitPrefabChecker>("Exhibit Prefab Checker");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Exhibit Prefab Checker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Checks for OBJ_ prefabs that don't have corresponding Exhibit prefabs in Resources/Exh/Prefabs", MessageType.Info);
        EditorGUILayout.Space(10);

        // Check button
        if (GUILayout.Button("Check for Missing Exhibit Prefabs", GUILayout.Height(30)))
        {
            CheckForMissingPrefabs();
        }

        EditorGUILayout.Space(10);

        // Status message
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }

        // Results section
        if (_showResults)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Missing prefabs section
            if (_missingPrefabs.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Missing Exhibit Prefabs ({_missingPrefabs.Count}):", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("These OBJ_ prefabs don't have corresponding Exhibit prefabs in Resources/Exh/Prefabs", MessageType.Warning);
                
                foreach (var missing in _missingPrefabs)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {missing}", GUILayout.ExpandWidth(true));
                    
                    // Button to ping the OBJ_ prefab
                    var objPrefabPath = $"{ObjPrefabsFolder}/{missing}.prefab";
                    var objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(objPrefabPath);
                    if (objPrefab != null && GUILayout.Button("Ping", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.PingObject(objPrefab);
                        Selection.activeObject = objPrefab;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Extra prefabs section
            if (_extraPrefabs.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Extra Exhibit Prefabs ({_extraPrefabs.Count}):", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("These Exhibit prefabs don't have corresponding OBJ_ prefabs", MessageType.Info);
                
                foreach (var extra in _extraPrefabs)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {extra}", GUILayout.ExpandWidth(true));
                    
                    // Button to ping the Exhibit prefab
                    var exhibitPrefabPath = $"{ExhibitPrefabsFolder}/{extra}.prefab";
                    var exhibitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(exhibitPrefabPath);
                    if (exhibitPrefab != null && GUILayout.Button("Ping", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.PingObject(exhibitPrefab);
                        Selection.activeObject = exhibitPrefab;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Summary
            EditorGUILayout.Space(10);
            var totalObjPrefabs = GetObjPrefabNames().Count;
            var totalExhibitPrefabs = GetExhibitPrefabNames().Count;
            
            EditorGUILayout.LabelField("Summary:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• Total OBJ_ prefabs: {totalObjPrefabs}");
            EditorGUILayout.LabelField($"• Total Exhibit prefabs: {totalExhibitPrefabs}");
            EditorGUILayout.LabelField($"• Missing Exhibit prefabs: {_missingPrefabs.Count}");
            EditorGUILayout.LabelField($"• Extra Exhibit prefabs: {_extraPrefabs.Count}");

            if (_missingPrefabs.Count == 0 && _extraPrefabs.Count == 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("✅ All OBJ_ prefabs have corresponding Exhibit prefabs!", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void CheckForMissingPrefabs()
    {
        _missingPrefabs.Clear();
        _extraPrefabs.Clear();
        _showResults = true;

        try
        {
            var objPrefabNames = GetObjPrefabNames();
            var exhibitPrefabNames = GetExhibitPrefabNames();

            // Check for missing exhibit prefabs
            foreach (var objName in objPrefabNames)
            {
                var expectedExhibitName = GetExpectedExhibitName(objName);
                if (!exhibitPrefabNames.Contains(expectedExhibitName))
                {
                    _missingPrefabs.Add(objName);
                }
            }

            // Check for extra exhibit prefabs (exhibit prefabs without corresponding OBJ_ prefabs)
            foreach (var exhibitName in exhibitPrefabNames)
            {
                // Skip template prefabs
                if (exhibitName.StartsWith("ExhTemp_"))
                    continue;

                var expectedObjName = GetExpectedObjName(exhibitName);
                if (!objPrefabNames.Contains(expectedObjName))
                {
                    _extraPrefabs.Add(exhibitName);
                }
            }

            // Update status message
            if (_missingPrefabs.Count == 0 && _extraPrefabs.Count == 0)
            {
                _statusMessage = "✅ All OBJ_ prefabs have corresponding Exhibit prefabs!";
            }
            else
            {
                _statusMessage = $"Found {_missingPrefabs.Count} missing Exhibit prefabs and {_extraPrefabs.Count} extra Exhibit prefabs.";
            }

            // Log results to console
            LogResults();

            Repaint();
        }
        catch (System.Exception ex)
        {
            _statusMessage = $"Error: {ex.Message}";
            Debug.LogError($"ExhibitPrefabChecker error: {ex}");
        }
    }

    private List<string> GetObjPrefabNames()
    {
        var names = new List<string>();
        
        if (!Directory.Exists(ObjPrefabsFolder))
        {
            Debug.LogError($"OBJ_ prefabs folder not found: {ObjPrefabsFolder}");
            return names;
        }

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { ObjPrefabsFolder });
        
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path);
            
            if (name.StartsWith("OBJ_"))
            {
                names.Add(name);
            }
        }

        return names.OrderBy(x => x).ToList();
    }

    private List<string> GetExhibitPrefabNames()
    {
        var names = new List<string>();
        
        if (!Directory.Exists(ExhibitPrefabsFolder))
        {
            Debug.LogError($"Exhibit prefabs folder not found: {ExhibitPrefabsFolder}");
            return names;
        }

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { ExhibitPrefabsFolder });
        
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path);
            names.Add(name);
        }

        return names.OrderBy(x => x).ToList();
    }

    private void LogResults()
    {
        Debug.Log("=== Exhibit Prefab Checker Results ===");
        
        var totalObjPrefabs = GetObjPrefabNames().Count;
        var totalExhibitPrefabs = GetExhibitPrefabNames().Count;
        
        Debug.Log($"Total OBJ_ prefabs: {totalObjPrefabs}");
        Debug.Log($"Total Exhibit prefabs: {totalExhibitPrefabs}");
        
        if (_missingPrefabs.Count > 0)
        {
            Debug.LogWarning($"Missing Exhibit prefabs ({_missingPrefabs.Count}):");
            foreach (var missing in _missingPrefabs)
            {
                Debug.LogWarning($"  • {missing} -> {GetExpectedExhibitName(missing)}");
            }
        }
        
        if (_extraPrefabs.Count > 0)
        {
            Debug.Log($"Extra Exhibit prefabs ({_extraPrefabs.Count}):");
            foreach (var extra in _extraPrefabs)
            {
                Debug.Log($"  • {extra} (no corresponding {GetExpectedObjName(extra)})");
            }
        }
        
        if (_missingPrefabs.Count == 0 && _extraPrefabs.Count == 0)
        {
            Debug.Log("✅ All OBJ_ prefabs have corresponding Exhibit prefabs!");
        }
        
        Debug.Log("=== End Results ===");
    }

    private string GetExpectedExhibitName(string objName)
    {
        // Handle special naming cases
        switch (objName)
        {
            case "OBJ_ArtOfWar":
                return "TheArtOfWar";
            case "OBJ_Blueprint":
                return "Blueprints";
            default:
                return objName.Replace("OBJ_", "");
        }
    }

    private string GetExpectedObjName(string exhibitName)
    {
        // Handle special naming cases
        switch (exhibitName)
        {
            case "TheArtOfWar":
                return "OBJ_ArtOfWar";
            case "Blueprints":
                return "OBJ_Blueprint";
            default:
                return "OBJ_" + exhibitName;
        }
    }
}
