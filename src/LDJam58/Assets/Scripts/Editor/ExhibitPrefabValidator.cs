using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    public class ExhibitPrefabValidator : IPreprocessBuildWithReport
{
    private const string PrefabsFolder = "Assets/Resources/Exh/Prefabs";
    private const string CsvPath = "Assets/exhibit_data.csv";
    private const string WallsLayerName = "Walls";
    private const string GroundLayerName = "Ground";
    private const int WallsLayer = 8;
    private const int GroundLayer = 3;
    
    // Placeholder prefabs that should be excluded from name validation
    private static readonly string[] PlaceholderPrefixes = { "ExhTemp_" };

    public int callbackOrder => 0;

    [MenuItem("Tools/QA/Validate Exhibit Prefabs %&v")]
    public static void ValidateAllPrefabs()
    {
        var validator = new ExhibitPrefabValidator();
        var result = validator.ValidatePrefabs();
        
        if (result.IsValid)
        {
            Debug.Log($"✅ All exhibit prefabs are valid! ({result.ValidCount} prefabs checked)");
        }
        else
        {
            Debug.LogError($"❌ Validation failed! {result.ErrorCount} errors found:");
            foreach (var error in result.Errors)
            {
                Debug.LogError($"  • {error}");
            }
        }
    }

    [MenuItem("Tools/QA/Fix Exhibit Prefab Layers")]
    public static void FixAllPrefabLayers()
    {
        var validator = new ExhibitPrefabValidator();
        var result = validator.FixPrefabLayers();
        
        Debug.Log($"🔧 Fixed layers for {result.FixedCount} prefabs. {result.ErrorCount} errors encountered.");
        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                Debug.LogError($"  • {error}");
            }
        }
    }

    [MenuItem("Tools/QA/Validate & Fix All Exhibit Prefabs")]
    public static void ValidateAndFixAllPrefabs()
    {
        Debug.Log("🔍 Validating exhibit prefabs...");
        var validator = new ExhibitPrefabValidator();
        var validationResult = validator.ValidatePrefabs();
        
        if (validationResult.IsValid)
        {
            Debug.Log($"✅ All prefabs are valid! ({validationResult.ValidCount} prefabs checked)");
            return;
        }
        
        Debug.Log($"❌ Found {validationResult.ErrorCount} validation errors. Attempting to fix layers...");
        var fixResult = validator.FixPrefabLayers();
        
        if (fixResult.FixedCount > 0)
        {
            Debug.Log($"🔧 Fixed {fixResult.FixedCount} prefabs. Re-running validation...");
            var revalidationResult = validator.ValidatePrefabs();
            
            if (revalidationResult.IsValid)
            {
                Debug.Log($"✅ All prefabs are now valid! ({revalidationResult.ValidCount} prefabs checked)");
            }
            else
            {
                Debug.LogError($"❌ Still have {revalidationResult.ErrorCount} validation errors after fixes:");
                foreach (var error in revalidationResult.Errors)
                {
                    Debug.LogError($"  • {error}");
                }
            }
        }
        else
        {
            Debug.LogError("❌ No layer fixes were applied. Manual intervention required.");
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("🔍 Running exhibit prefab validation before build...");
        var result = ValidatePrefabs();
        
        if (!result.IsValid)
        {
            Debug.LogError("❌ Build cancelled due to exhibit prefab validation errors:");
            foreach (var error in result.Errors)
            {
                Debug.LogError($"  • {error}");
            }
            throw new BuildFailedException("Exhibit prefab validation failed. See console for details.");
        }
        
        Debug.Log($"✅ Exhibit prefab validation passed! ({result.ValidCount} prefabs validated)");
    }

    public ValidationResult ValidatePrefabs()
    {
        var result = new ValidationResult();
        var exhibitNames = LoadExhibitNames();
        
        if (!Directory.Exists(PrefabsFolder))
        {
            result.Errors.Add($"Prefabs folder not found: {PrefabsFolder}");
            return result;
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder });
        
        foreach (var guid in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null) continue;
            
            var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            
            // Validate prefab name against CSV data (skip placeholders)
            if (!IsPlaceholderPrefab(prefabName))
            {
                if (!exhibitNames.Contains(prefabName.ToLower()))
                {
                    result.Errors.Add($"Prefab '{prefabName}' does not match any exhibit in CSV data");
                }
            }
            
            // Validate layers
            var placementBase = FindPlacementBase(prefab);
            if (placementBase == null)
            {
                result.Errors.Add($"Prefab '{prefabName}' missing PlacementBase child");
                continue;
            }
            
            // Check PlacementBase layer
            if (placementBase.layer != WallsLayer)
            {
                result.Errors.Add($"Prefab '{prefabName}' PlacementBase is on layer {placementBase.layer} (should be {WallsLayer} - {WallsLayerName})");
            }
            
            // Check children layers
            var children = GetChildrenRecursively(placementBase.transform);
            foreach (var child in children)
            {
                if (child.gameObject.layer != GroundLayer)
                {
                    result.Errors.Add($"Prefab '{prefabName}' child '{child.name}' is on layer {child.gameObject.layer} (should be {GroundLayer} - {GroundLayerName})");
                }
            }
            
            result.ValidCount++;
        }
        
        result.ErrorCount = result.Errors.Count;
        return result;
    }

    public FixResult FixPrefabLayers()
    {
        var result = new FixResult();
        
        if (!Directory.Exists(PrefabsFolder))
        {
            result.Errors.Add($"Prefabs folder not found: {PrefabsFolder}");
            return result;
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolder });
        var modifiedPrefabs = new List<string>();
        
        foreach (var guid in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null) continue;
            
            var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            var placementBase = FindPlacementBase(prefab);
            
            if (placementBase == null)
            {
                result.Errors.Add($"Prefab '{prefabName}' missing PlacementBase child");
                continue;
            }
            
            var modified = false;
            
            // Fix PlacementBase layer
            if (placementBase.layer != WallsLayer)
            {
                Undo.RecordObject(placementBase, $"Fix {prefabName} PlacementBase layer");
                placementBase.layer = WallsLayer;
                modified = true;
            }
            
            // Fix children layers
            var children = GetChildrenRecursively(placementBase.transform);
            foreach (var child in children)
            {
                if (child.gameObject.layer != GroundLayer)
                {
                    Undo.RecordObject(child.gameObject, $"Fix {prefabName} child layer");
                    child.gameObject.layer = GroundLayer;
                    modified = true;
                }
            }
            
            if (modified)
            {
                modifiedPrefabs.Add(prefabPath);
                EditorUtility.SetDirty(prefab);
                result.FixedCount++;
            }
        }
        
        // Save all modified prefabs
        if (modifiedPrefabs.Count > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"💾 Saved {modifiedPrefabs.Count} modified prefabs");
        }
        
        return result;
    }

    private HashSet<string> LoadExhibitNames()
    {
        var exhibitNames = new HashSet<string>();
        
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"CSV file not found: {CsvPath}");
            return exhibitNames;
        }
        
        try
        {
            var exhibits = ExhibitCsvLoader.LoadFromCsv(CsvPath);
            foreach (var exhibit in exhibits)
            {
                exhibitNames.Add(exhibit.FileFriendlyName.ToLower());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load exhibit data: {e.Message}");
        }
        
        return exhibitNames;
    }

    private bool IsPlaceholderPrefab(string prefabName)
    {
        return PlaceholderPrefixes.Any(prefix => prefabName.StartsWith(prefix));
    }

    private GameObject FindPlacementBase(GameObject prefab)
    {
        // Look for child that starts with "PlacementBase"
        var children = GetChildrenRecursively(prefab.transform);
        return children.FirstOrDefault(child => child.name.StartsWith("PlacementBase"))?.gameObject;
    }

    private List<Transform> GetChildrenRecursively(Transform parent)
    {
        var children = new List<Transform>();
        
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            children.Add(child);
            children.AddRange(GetChildrenRecursively(child));
        }
        
        return children;
    }

    public class ValidationResult
    {
        public List<string> Errors { get; } = new List<string>();
        public int ValidCount { get; set; }
        public int ErrorCount { get; set; }
        public bool IsValid => ErrorCount == 0;
    }

    public class FixResult
    {
        public List<string> Errors { get; } = new List<string>();
        public int FixedCount { get; set; }
        public int ErrorCount => Errors.Count;
    }
    }
}
