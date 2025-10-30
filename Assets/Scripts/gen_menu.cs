using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor.SceneManagement;

public class GenerationWindow : EditorWindow
{
    private int selectedTab = 0;
    private string[] tabs = { "Generate", "Generations", "Settings" };

    // Generate tab fields
    private string promptText = "";
    private string generationName = "";
    private bool use_asset_project_generator_class = true;

    // Generations tab
    private Vector2 scrollPos;
    private string[] generationOptions;
    private int selectedGenerationIndex = -0;
    private string[] generationFiles;

    public static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    public static readonly string assetProject = new DirectoryInfo(ProjectRoot).Name;
    private string rootPath = "../..";  // ".." means one folder above Assets (the project root)
    private string[] folderOptions;
    private int selectedFolderIndex = 0;


    [MenuItem("Gen Menu/Generation Window %#g")] // Ctrl/Cmd + Shift + G
    private static void OpenWindow()
    {
        var window = GetWindow<GenerationWindow>("Generation Window");
        window.minSize = new Vector2(500, 300);
        window.RefreshFolderList();
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
        
        use_asset_project_generator_class = EditorGUILayout.Toggle($"Use {assetProject}", true);

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
                UnityEngine.Debug.Log($"Generating '{generationName}' with prompt: {promptText}");

                Generate();

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

        if (GUILayout.Button("Refresh Generation List"))
            RefreshGenerationsList();
        if (generationOptions == null || generationOptions.Length == 0)
        {
            EditorGUILayout.LabelField("No prior generations found.");
            return;
        }

        selectedGenerationIndex = EditorGUILayout.Popup("Scene:", selectedGenerationIndex, generationOptions);
        EditorGUILayout.LabelField("Selected Generation Path:");
        EditorGUILayout.TextField(GetSelectedGenerationPath());
        // Example button
        if (GUILayout.Button("Open Scene"))
        {
            string scenePath = GetSelectedGenerationPath();

        // Check if the scene exists
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Select an Asset Project", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (folderOptions == null || folderOptions.Length == 0)
        {
            EditorGUILayout.HelpBox("No folders found.", MessageType.Info);
            return;
        }

        // Dropdown (popup)
        selectedFolderIndex = EditorGUILayout.Popup("Asset Project:", selectedFolderIndex, folderOptions);

        // Show the selected folder path
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected Asset Project Path:");
        EditorGUILayout.TextField(GetSelectedFolderPath());
        // END
        if (GUILayout.Button("Open Asset Project"))
        {
            EditorUtility.DisplayDialog("Error", "Are you sure you want to open a new Unity project?", "Continue");
            UnityEngine.Debug.Log("Selected Generation: " + GetSelectedGenerationPath());
        }

        EditorGUILayout.LabelField("Settings will go here...");
        EditorGUILayout.Toggle("Example Toggle", true);
        EditorGUILayout.FloatField("Example Float", 1.0f);
    }

    private string GetSelectedFolderPath()
    {
        string assetProjects = Path.GetFullPath(Path.Combine(Application.dataPath, rootPath));
        if (selectedFolderIndex <= 0)
            return assetProjects;

        return Path.Combine(assetProjects, folderOptions[selectedFolderIndex]);
    }

    private string GetSelectedGenerationPath()
    {   
        string generationsRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "Generations"));
        if (selectedGenerationIndex < 0)
            return generationsRoot;
        return Path.ChangeExtension(Path.Combine(generationsRoot, generationOptions[selectedGenerationIndex]), ".unity");
    }

    private void RefreshFolderList()
    {
        string assetProjects = Path.GetFullPath(Path.Combine(Application.dataPath, rootPath));

        if (!Directory.Exists(assetProjects))
        {
            folderOptions = new string[0];
            return;
        }

        folderOptions = Directory.GetDirectories(assetProjects, "*", SearchOption.TopDirectoryOnly)
                                 .Select(Path.GetFileName)
                                 .Prepend(Path.GetFullPath(Path.Combine(Application.dataPath, "..")))
                                 .ToArray();

        selectedFolderIndex = 0;
    }

    private void RefreshGenerationsList()
    {
        string generationsRoot = Path.Combine(Application.dataPath, "Generations");
        if (!Directory.Exists(generationsRoot))
        {
            UnityEngine.Debug.Log("Dont see generations...");
            folderOptions = new string[0];
            return;
        }

        generationOptions = Directory.GetFiles(generationsRoot, "*.unity", SearchOption.TopDirectoryOnly)
                             .Select(Path.GetFileNameWithoutExtension)
                             .ToArray();

        selectedGenerationIndex = -1;
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
            UnityEngine.Debug.Log("Opened calibration scene: " + scenePath);
        }
    }

    private void Generate()
    {
        string scriptPath = "../../../../../Backend/generate.sh"; // absolute or relative
        // Make this a reative path...
        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError($"Script not found: {scriptPath}");
            return;
        }

        // Construct the bash command arguments
        string bashArgs = $"\"{scriptPath}\" \"{assetProject}\" \"{promptText}\" \"{generationName}\" \"{use_asset_project_generator_class.ToString()}\"";


        ProcessStartInfo psi = new ProcessStartInfo()
        {
            FileName = "/bin/bash",
            Arguments = bashArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();

            UnityEngine.Debug.Log($"✅ Bash output:\n{output}");
            if (!string.IsNullOrEmpty(errors))
                UnityEngine.Debug.LogWarning($"⚠️ Bash errors:\n{errors}");
        }
    }
}
