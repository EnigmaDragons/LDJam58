using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class PrefabRendererWindow : EditorWindow
{
    private GameObject prefabToRender;
    private Vector3 renderPosition = new Vector3(1000, 0, 1000);
    private float cameraDistance = 5f;
    private float cameraHeight = 2f;
    private string outputFileName = "prefab_render";
    private bool isRendering = false;
    private string statusMessage = "";
    private Vector2 scrollPosition;
    
    private const string PREF_RENDER_POSITION_X = "PrefabRenderer_PositionX";
    private const string PREF_RENDER_POSITION_Y = "PrefabRenderer_PositionY";
    private const string PREF_RENDER_POSITION_Z = "PrefabRenderer_PositionZ";
    private const string PREF_CAMERA_DISTANCE = "PrefabRenderer_CameraDistance";
    private const string PREF_CAMERA_HEIGHT = "PrefabRenderer_CameraHeight";
    private const string PREF_OUTPUT_FILENAME = "PrefabRenderer_OutputFilename";

    [MenuItem("Tools/Prefab Renderer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabRendererWindow>("Prefab Renderer");
        window.minSize = new Vector2(350, 400);
    }

    private void OnEnable()
    {
        renderPosition.x = EditorPrefs.GetFloat(PREF_RENDER_POSITION_X, 1000f);
        renderPosition.y = EditorPrefs.GetFloat(PREF_RENDER_POSITION_Y, 0f);
        renderPosition.z = EditorPrefs.GetFloat(PREF_RENDER_POSITION_Z, 1000f);
        cameraDistance = EditorPrefs.GetFloat(PREF_CAMERA_DISTANCE, 5f);
        cameraHeight = EditorPrefs.GetFloat(PREF_CAMERA_HEIGHT, 2f);
        outputFileName = EditorPrefs.GetString(PREF_OUTPUT_FILENAME, "prefab_render");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Renderer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Renders a prefab in a distant corner and saves as a 400x400 JPG image.", MessageType.Info);
        EditorGUILayout.Space(10);

        // Prefab selection
        EditorGUI.BeginChangeCheck();
        prefabToRender = (GameObject)EditorGUILayout.ObjectField(
            "Prefab to Render", 
            prefabToRender, 
            typeof(GameObject), 
            false
        );
        if (EditorGUI.EndChangeCheck())
        {
            // Auto-generate filename based on prefab name
            if (prefabToRender != null)
            {
                outputFileName = SanitizeFileName(prefabToRender.name);
                EditorPrefs.SetString(PREF_OUTPUT_FILENAME, outputFileName);
            }
        }

        EditorGUILayout.Space(10);

        // Render position
        EditorGUILayout.LabelField("Render Position", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        renderPosition = EditorGUILayout.Vector3Field("Position", renderPosition);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat(PREF_RENDER_POSITION_X, renderPosition.x);
            EditorPrefs.SetFloat(PREF_RENDER_POSITION_Y, renderPosition.y);
            EditorPrefs.SetFloat(PREF_RENDER_POSITION_Z, renderPosition.z);
        }

        EditorGUILayout.Space(10);

        // Camera settings
        EditorGUILayout.LabelField("Camera Settings", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        cameraDistance = EditorGUILayout.FloatField("Distance from Object", cameraDistance);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat(PREF_CAMERA_DISTANCE, cameraDistance);
        }

        EditorGUI.BeginChangeCheck();
        cameraHeight = EditorGUILayout.FloatField("Camera Height", cameraHeight);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat(PREF_CAMERA_HEIGHT, cameraHeight);
        }

        EditorGUILayout.Space(10);

        // Output filename
        EditorGUILayout.LabelField("Output Settings", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        outputFileName = EditorGUILayout.TextField("Output Filename", outputFileName);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PREF_OUTPUT_FILENAME, outputFileName);
        }

        EditorGUILayout.Space(10);

        // Render button
        GUI.enabled = !isRendering && prefabToRender != null;
        if (GUILayout.Button(isRendering ? "Rendering..." : "Render Prefab", GUILayout.Height(30)))
        {
            RenderPrefab();
        }
        GUI.enabled = true;

        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RenderPrefab()
    {
        if (prefabToRender == null)
        {
            statusMessage = "ERROR: Please select a prefab to render!";
            EditorUtility.DisplayDialog("Error", "Please select a prefab to render.", "OK");
            return;
        }

        isRendering = true;
        statusMessage = "Setting up render scene...";
        Repaint();

        try
        {
            // Store the original scene
            var originalScene = SceneManager.GetActiveScene();
            var originalScenePath = originalScene.path;
            
            // Create a temporary scene for rendering
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create the prefab instance
            var prefabInstance = PrefabUtility.InstantiatePrefab(prefabToRender) as GameObject;
            prefabInstance.transform.position = renderPosition;
            
            // Create a camera to look at the prefab
            var cameraGO = new GameObject("RenderCamera");
            var camera = cameraGO.AddComponent<Camera>();
            
            // Position camera to look at the prefab
            var lookDirection = Vector3.forward;
            var cameraPos = renderPosition + lookDirection * cameraDistance + Vector3.up * cameraHeight;
            cameraGO.transform.position = cameraPos;
            cameraGO.transform.LookAt(renderPosition + Vector3.up * cameraHeight * 0.5f);
            
            // Set up camera for rendering
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            
            // Create a render texture
            var renderTexture = new RenderTexture(400, 400, 24);
            camera.targetTexture = renderTexture;
            
            statusMessage = "Rendering...";
            Repaint();
            
            // Force render
            camera.Render();
            
            // Read the render texture
            RenderTexture.active = renderTexture;
            var texture2D = new Texture2D(400, 400, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, 400, 400), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            
            // Convert to JPG and save
            var jpgData = texture2D.EncodeToJPG(80);
            
            // Create output directory if it doesn't exist
            var outputDir = Path.Combine(Application.dataPath, "Generated");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // Save the image
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{outputFileName}_{timestamp}.jpg";
            var filepath = Path.Combine(outputDir, filename);
            File.WriteAllBytes(filepath, jpgData);
            
            // Clean up
            DestroyImmediate(texture2D);
            DestroyImmediate(renderTexture);
            DestroyImmediate(prefabInstance);
            DestroyImmediate(cameraGO);
            
            // Return to the original scene
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath);
            }
            else
            {
                // If the original scene was unsaved, create a new empty scene
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            
            statusMessage = $"Successfully rendered and saved to: {filepath}";
            AssetDatabase.Refresh();
            
            Debug.Log($"Prefab rendered and saved to: {filepath}");
            EditorUtility.DisplayDialog("Render Complete", $"Prefab rendered successfully!\nSaved to: {filename}", "OK");
        }
        catch (System.Exception ex)
        {
            statusMessage = $"ERROR: {ex.Message}";
            Debug.LogError($"Prefab render error: {ex}");
            EditorUtility.DisplayDialog("Error", $"Failed to render prefab:\n{ex.Message}", "OK");
        }
        finally
        {
            isRendering = false;
            Repaint();
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters and convert to lowercase
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = fileName.ToLower();
        
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
}
