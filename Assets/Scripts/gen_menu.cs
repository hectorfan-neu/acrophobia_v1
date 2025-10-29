using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

public class GenerationWindow : EditorWindow
{
    private int selectedTab = 0;
    private string[] tabs = { "Generate", "Generations", "Settings" };

    // Generate tab fields
    private string promptText = "";
    private string generationName = "";

    // Generations tab
    private Vector2 scrollPos;
    private string[] generationOptions;
    private int selectedGenerationIndex = 0;
    private string[] generationFiles;

    private string rootPath = "..";  // ".." means one folder above Assets (the project root)
    private string[] folderOptions;
    private int selectedFolderIndex = 0;

    [MenuItem("Gen Menu/Generation Window %#g")] // Ctrl/Cmd + Shift + G
    private static void OpenWindow()
    {
        var window = GetWindow<GenerationWindow>("Generation Window");
        window.minSize = new Vector2(500, 300);
        window.RefreshGenerationsList();
    }



    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // Sidebar
        EditorGUILayout.BeginVertical(GUILayout.Width(120));
        selectedTab = GUILayout.SelectionGrid(selectedTab, tabs, 1);
        EditorGUILayout.EndVertical();

        // Main content
        EditorGUILayout.BeginVertical();
        switch (selectedTab)
        {
            case 0:
                DrawGenerateTab();
                break;
            case 1:
                DrawGenerationsTab();
                break;
            case 2:
                DrawSettingsTab();
                break;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGenerateTab()
    {
        EditorGUILayout.LabelField("Generate New Prompt", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        promptText = EditorGUILayout.TextField("Prompt:", promptText);
        generationName = EditorGUILayout.TextField("Scene name:", generationName);

        EditorGUILayout.LabelField("Select a Folder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Refresh folders button
        if (GUILayout.Button("Refresh Folder List"))
            RefreshFolderList();

        if (folderOptions == null || folderOptions.Length == 0)
        {
            EditorGUILayout.HelpBox("No folders found.", MessageType.Info);
            return;
        }

        // Dropdown (popup)
        selectedFolderIndex = EditorGUILayout.Popup("Folder:", selectedFolderIndex, folderOptions);

        // Show the selected folder path
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected Folder Path:");
        EditorGUILayout.TextField(GetSelectedFolderPath());

        // Example button
        if (GUILayout.Button("Do Something With Folder"))
        {
            Debug.Log("Selected Folder: " + GetSelectedFolderPath());
        }

        EditorGUILayout.LabelField("Parameters here...");
        EditorGUILayout.Toggle("Example Toggle", true);
        EditorGUILayout.FloatField("Example Float", 1.0f);

        if (GUILayout.Button("Generate"))
        {
            if (string.IsNullOrEmpty(generationName) || string.IsNullOrEmpty(promptText))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a name for the generation.", "OK");
            }
            else
            {
                // Here you would implement your generation logic
                Debug.Log($"Generating '{generationName}' with prompt: {promptText}");

                // For example, save a placeholder file
                string path = Path.Combine(Application.dataPath, "Generations");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


                RefreshGenerationsList();
            }
        }
    }

    private void DrawGenerationsTab()
    {
        EditorGUILayout.LabelField("Prior Generations", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (generationFiles == null || generationFiles.Length == 0)
        {
            EditorGUILayout.LabelField("No prior generations found.");
            return;
        }

        selectedGenerationIndex = EditorGUILayout.Popup("Scene:", selectedGenerationIndex, generationOptions);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (string file in generationFiles)
        {
            if (GUILayout.Button(Path.GetFileNameWithoutExtension(file)))
            {
                // Load the generation (e.g., open the scene or load content)
                string content = File.ReadAllText(file);
                Debug.Log($"Loaded generation '{Path.GetFileNameWithoutExtension(file)}': {content}");
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings will go here...");
        EditorGUILayout.Toggle("Example Toggle", true);
        EditorGUILayout.FloatField("Example Float", 1.0f);
    }

    private string GetSelectedFolderPath()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, rootPath));
        if (selectedFolderIndex <= 0)
            return projectRoot;

        return Path.Combine(projectRoot, folderOptions[selectedFolderIndex]);
    }

    private void RefreshFolderList()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, rootPath));

        if (!Directory.Exists(projectRoot))
        {
            folderOptions = new string[0];
            return;
        }

        folderOptions = Directory.GetDirectories(projectRoot, "*", SearchOption.TopDirectoryOnly)
                                 .Select(Path.GetFileName)
                                 .Prepend("<Project Root>")
                                 .ToArray();

        selectedFolderIndex = 0;
    }

    private void RefreshGenerationsList()
    {
        string path = Path.Combine(Application.dataPath, "Generations");
        if (!Directory.Exists(path))
        {
            generationFiles = new string[0];
            return;
        }

        generationFiles = Directory.GetFiles(path, "*.txt");
    }

    [MenuItem("Gen Menu/Calibrate...")]
    private static void OpenCalibrationScene()
    {
        string scenePath = "Assets/Scenes/calibrate.unity";

        // Check if the scene exists
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (sceneAsset == null)
        {
            EditorUtility.DisplayDialog("Scene Not Found",
                $"Could not find the calibration scene at:\n{scenePath}\n\nMake sure it exists in your project.",
                "OK");
            return;
        }

        // Prompt to save current changes
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log("Opened calibration scene: " + scenePath);
        }
    }
}
