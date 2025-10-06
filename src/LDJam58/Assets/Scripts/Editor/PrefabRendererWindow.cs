using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class PrefabRendererWindow : EditorWindow
{
    private GameObject _prefabToRender;
    private bool _isRendering;
    private string _statusMessage;
    private Vector2 _scrollPosition;
    
    private const string PhotostudioScenePath = "Assets/Scenes/Pipeline/PhotoStudioV2.unity";
    
    // Batch processing variables
    private List<GameObject> _prefabsToProcess;
    private int _currentPrefabIndex;

    [MenuItem("Tools/Prefab Renderer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabRendererWindow>("Prefab Renderer");
        window.minSize = new Vector2(350, 300);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Renderer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Renders a prefab in PhotoStudio scene using Main Camera and PhotoSpot. Saves to Resources/Exh/Sprites.", MessageType.Info);
        EditorGUILayout.Space(10);

        // Prefab selection
        _prefabToRender = (GameObject)EditorGUILayout.ObjectField(
            "Prefab to Render", 
            _prefabToRender, 
            typeof(GameObject), 
            false
        );

        EditorGUILayout.Space(10);

        // Render button
        GUI.enabled = !_isRendering && _prefabToRender != null;
        if (GUILayout.Button(_isRendering ? "Rendering..." : "Render Prefab", GUILayout.Height(30)))
        {
            RenderPrefab();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Photograph All button
        GUI.enabled = !_isRendering;
        if (GUILayout.Button(_isRendering ? "Processing..." : "Photograph All Prefabs", GUILayout.Height(30)))
        {
            PhotographAllPrefabs();
        }
        GUI.enabled = true;

        // Status message
        if (!string.IsNullOrEmpty(_statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RenderPrefab()
    {
        if (_prefabToRender == null)
        {
            _statusMessage = "ERROR: Please select a prefab to render!";
            EditorUtility.DisplayDialog("Error", "Please select a prefab to render.", "OK");
            return;
        }

        _isRendering = true;
        _statusMessage = "Setting up render scene...";
        Repaint();

        try
        {
            // Store the original scene
            var originalScene = SceneManager.GetActiveScene();
            var originalScenePath = originalScene.path;
            
            // Load PhotoStudio scene
            _statusMessage = "Loading PhotoStudio scene...";
            Repaint();
            EditorSceneManager.OpenScene(PhotostudioScenePath, OpenSceneMode.Single);
            
            // Force update to ensure scene is loaded
            EditorApplication.QueuePlayerLoopUpdate();
            
            // Find Main Camera
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _statusMessage = "ERROR: Main Camera not found in PhotoStudio scene!";
                EditorUtility.DisplayDialog("Error", "Main Camera not found in PhotoStudio scene!", "OK");
                _isRendering = false;
                Repaint();
                return;
            }
            
            // Find PhotoSpot
            var photoSpot = GameObject.FindGameObjectWithTag("PhotoSpot");
            if (photoSpot == null)
            {
                _statusMessage = "ERROR: GameObject with 'PhotoSpot' tag not found!";
                EditorUtility.DisplayDialog("Error", "GameObject with 'PhotoSpot' tag not found!", "OK");
                _isRendering = false;
                Repaint();
                return;
            }
            
            _statusMessage = "Spawning prefab...";
            Repaint();
            
            // Create the prefab instance at PhotoSpot position
            var prefabInstance = PrefabUtility.InstantiatePrefab(_prefabToRender) as GameObject;
            if (prefabInstance != null)
            {
                prefabInstance.transform.position = photoSpot.transform.position;
                prefabInstance.transform.rotation = photoSpot.transform.rotation;
                
                // Apply placement offset based on child size markers
                var placementOffset = CalculatePlacementOffset(prefabInstance);
                if (placementOffset != Vector3.zero)
                {
                    prefabInstance.transform.position += placementOffset;
                }
                
                // Deactivate PlacementBase children
                DeactivatePlacementBaseChildren(prefabInstance);

                // If prefab contains particle systems, simulate 3 seconds before capture
                HandleParticleSystems(prefabInstance);
            }
            
            // Force update to ensure prefab is positioned
            EditorApplication.QueuePlayerLoopUpdate();
            EditorApplication.QueuePlayerLoopUpdate();
            
            _statusMessage = "Rendering...";
            Repaint();
            
            // Set up camera for rendering
            var renderTexture = new RenderTexture(400, 400, 24);
            mainCamera.targetTexture = renderTexture;
            
            // Force render
            mainCamera.Render();
            
            // Read the render texture
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(400, 400, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, 400, 400), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            
            // Convert to JPG and save
            var jpgData = texture2D.EncodeToJPG(80);
            
            // Create output directory if it doesn't exist
            var outputDir = Path.Combine(Application.dataPath, "Resources", "Exh", "Sprites");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // Save the image with prefab name
            var filename = SanitizeFileName(_prefabToRender.name) + ".jpg";
            var filepath = Path.Combine(outputDir, filename);
            File.WriteAllBytes(filepath, jpgData);
            
            // Configure sprite import settings for the new asset
            ConfigureSpriteImportSettings(filepath);
            
            // Clean up
            DestroyImmediate(texture2D);
            DestroyImmediate(renderTexture);
            if (prefabInstance != null)
            {
                DestroyImmediate(prefabInstance);
            }
            mainCamera.targetTexture = null;
            
            // Return to the original scene
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }
            
            _statusMessage = $"Successfully rendered and saved to: {filepath}";
            AssetDatabase.Refresh();
            
            Debug.Log($"Prefab rendered and saved to: {filepath}");
            EditorUtility.DisplayDialog("Render Complete", $"Prefab rendered successfully!\nSaved to: {filename}", "OK");
        }
        catch (System.Exception ex)
        {
            _statusMessage = $"ERROR: {ex.Message}";
            Debug.LogError($"Prefab render error: {ex}");
            EditorUtility.DisplayDialog("Error", $"Failed to render prefab:\n{ex.Message}", "OK");
        }
        finally
        {
            _isRendering = false;
            Repaint();
        }
    }
    
    private void PhotographAllPrefabs()
    {
        if (_isRendering)
        {
            return;
        }

        // Find all prefabs in Resources/Exh/Prefabs
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Exh/Prefabs" });
        _prefabsToProcess = new List<GameObject>();

        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && !prefab.name.StartsWith("Exh"))
            {
                _prefabsToProcess.Add(prefab);
            }
        }

        if (_prefabsToProcess.Count == 0)
        {
            _statusMessage = "No prefabs found to photograph!";
            EditorUtility.DisplayDialog("No Prefabs", "No prefabs found in Resources/Exh/Prefabs (excluding Exh prefabs).", "OK");
            return;
        }

        _isRendering = true;
        _currentPrefabIndex = 0;
        _statusMessage = $"Found {_prefabsToProcess.Count} prefabs to photograph. Starting batch process...";
        Repaint();

        // Start the batch processing
        EditorApplication.update += ProcessNextPrefab;
    }

    private void ProcessNextPrefab()
    {
        if (_currentPrefabIndex >= _prefabsToProcess.Count)
        {
            // All done
            EditorApplication.update -= ProcessNextPrefab;
            _isRendering = false;
            _statusMessage = "Batch photography complete!";
            Repaint();
            EditorUtility.DisplayDialog("Batch Complete", "All prefabs have been photographed successfully!", "OK");
            AssetDatabase.Refresh();
            return;
        }

        var currentPrefab = _prefabsToProcess[_currentPrefabIndex];
        _statusMessage = $"Photographing: {currentPrefab.name} ({_currentPrefabIndex + 1}/{_prefabsToProcess.Count})";
        Repaint();

        try
        {
            // Store the original scene
            var originalScene = SceneManager.GetActiveScene();
            var originalScenePath = originalScene.path;
            
            // Load PhotoStudio scene
            EditorSceneManager.OpenScene(PhotostudioScenePath, OpenSceneMode.Single);
            
            // Force update to ensure scene is loaded
            EditorApplication.QueuePlayerLoopUpdate();
            
            // Find Main Camera
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _statusMessage = $"ERROR: Main Camera not found for {currentPrefab.name}!";
                Repaint();
                _currentPrefabIndex++;
                return;
            }
            
            // Find PhotoSpot
            var photoSpot = GameObject.FindGameObjectWithTag("PhotoSpot");
            if (photoSpot == null)
            {
                _statusMessage = $"ERROR: PhotoSpot not found for {currentPrefab.name}!";
                Repaint();
                _currentPrefabIndex++;
                return;
            }
            
            // Create the prefab instance at PhotoSpot position
            var prefabInstance = PrefabUtility.InstantiatePrefab(currentPrefab) as GameObject;
            if (prefabInstance != null)
            {
                prefabInstance.transform.position = photoSpot.transform.position;
                prefabInstance.transform.rotation = photoSpot.transform.rotation;
                
                // Apply placement offset based on child size markers
                var placementOffset = CalculatePlacementOffset(prefabInstance);
                if (placementOffset != Vector3.zero)
                {
                    prefabInstance.transform.position += placementOffset;
                }

                // Deactivate PlacementBase children
                DeactivatePlacementBaseChildren(prefabInstance);

                // If prefab contains particle systems, simulate 3 seconds before capture
                HandleParticleSystems(prefabInstance);
            }
            
            // Force update to ensure prefab is positioned
            EditorApplication.QueuePlayerLoopUpdate();
            EditorApplication.QueuePlayerLoopUpdate();
            
            // Set up camera for rendering
            var renderTexture = new RenderTexture(400, 400, 24);
            mainCamera.targetTexture = renderTexture;
            
            // Force render
            mainCamera.Render();
            
            // Read the render texture
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(400, 400, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, 400, 400), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            
            // Convert to JPG and save
            var jpgData = texture2D.EncodeToJPG(80);
            
            // Create output directory if it doesn't exist
            var outputDir = Path.Combine(Application.dataPath, "Resources", "Exh", "Sprites");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // Save the image with prefab name
            var filename = SanitizeFileName(currentPrefab.name) + ".jpg";
            var filepath = Path.Combine(outputDir, filename);
            File.WriteAllBytes(filepath, jpgData);
            
            // Configure sprite import settings for the new asset
            ConfigureSpriteImportSettings(filepath);
            
            // Clean up
            DestroyImmediate(texture2D);
            DestroyImmediate(renderTexture);
            if (prefabInstance != null)
            {
                DestroyImmediate(prefabInstance);
            }
            mainCamera.targetTexture = null;
            
            // Return to the original scene
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }
            
            Debug.Log($"Prefab photographed: {currentPrefab.name}");
            
            // Move to next prefab
            _currentPrefabIndex++;
        }
        catch (System.Exception ex)
        {
            _statusMessage = $"ERROR photographing {currentPrefab.name}: {ex.Message}";
            Debug.LogError($"Prefab render error for {currentPrefab.name}: {ex}");
            _currentPrefabIndex++;
            Repaint();
        }
    }
    
    private void DeactivatePlacementBaseChildren(GameObject parent)
    {
        var childrenToDeactivate = new List<GameObject>();
        
        // Find all children with names starting with "PlacementBase"
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("PlacementBase"))
            {
                childrenToDeactivate.Add(child);
            }
        }
        
        // Deactivate found children
        foreach (var child in childrenToDeactivate)
        {
            child.SetActive(false);
        }
    }

    private Vector3 CalculatePlacementOffset(GameObject parent)
    {
        // Inspect direct children for specific size markers
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("PlacementBase-3x3"))
            {
                return new Vector3(-1f, 0f, -1f);
            }
            if (child.name.StartsWith("PlacementBase-3x2"))
            {
                return new Vector3(-1f, 0f, 0f);
            }
        }
        return Vector3.zero;
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters and convert to lowercase
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }
        
        // Replace spaces with underscores
        sanitized = sanitized.Replace(' ', '_');
        
        // Remove multiple consecutive underscores
        while (sanitized.Contains("__"))
        {
            sanitized = sanitized.Replace("__", "_");
        }
        
        // Trim underscores from start and end
        sanitized = sanitized.Trim('_');
        
        return string.IsNullOrEmpty(sanitized) ? "prefab_render" : sanitized;
    }
    
    private void ConfigureSpriteImportSettings(string filepath)
    {
        // Get the relative path from Assets folder
        var relativePath = filepath.Replace(Application.dataPath, "Assets");
        
        // Import the asset to make it available in the project
        AssetDatabase.ImportAsset(relativePath);
        
        // Get the texture importer
        var textureImporter = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (textureImporter != null)
        {
            // Configure as Sprite (2D and UI)
            textureImporter.textureType = TextureImporterType.Sprite;
            
            // Set sprite mode to Single
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            
            // Apply the changes
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
            
            Debug.Log($"Configured sprite import settings for: {relativePath}");
        }
    }

    private void HandleParticleSystems(GameObject root)
    {
        // If there are particle systems, simulate 3 seconds so effects are visible in the photo
        var firstPs = root.GetComponentInChildren<ParticleSystem>(true);
        if (firstPs == null)
        {
            return;
        }

        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            ps.Clear(true);
            // Simulate advances the particle system as if 3 seconds have passed
            ps.Simulate(3f, true, true, true);
        }

        // Ensure editor updates any visuals before rendering
        EditorApplication.QueuePlayerLoopUpdate();
    }
}
