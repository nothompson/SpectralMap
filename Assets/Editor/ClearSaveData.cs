using UnityEngine;
using UnityEditor;
using System.IO;

public class ClearSaveData : EditorWindow
{
    private static readonly string[] Files = new[]
    {
        "DeathLog.json",
        "Dialogue.json",
        "Doors.json",
        "Inventory.json",
        "Journal.json",
        "Spectrum.json",
    };

    [MenuItem("Tools/Clear Save Data")]
    public static void ShowWindow()
    {
        GetWindow<ClearSaveData>("Clear Save Data");
    }

    void OnGUI()
    {
        GUILayout.Label("Save Files", EditorStyles.boldLabel);

        foreach(var file in Files)
        {
            string path = Path.Combine(Application.persistentDataPath, file);
            bool exists = File.Exists(path);
            EditorGUI.BeginDisabledGroup(!exists);
            if(GUILayout.Button($"{(exists ? "✓" : "X")} Delete {file}"))
            {
                if(EditorUtility.DisplayDialog("Delete Save File", $"Delete {file}?", "Yes", "No"))
                {
                    File.Delete(path);
                    Debug.Log($"Deleted {path}");
                }
            }
            EditorGUI.EndDisabledGroup();
        }    

        GUILayout.Space(10);

        GUI.backgroundColor = Color.red;

        if(GUILayout.Button("Delete All Save Data"))
        {
            if(EditorUtility.DisplayDialog("Delete all data", "you sure?", "Yes!", "No.."))
            {
                ClearAll();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        if(GUILayout.Button("Open Save Folder"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }

    static void ClearAll()
    {
        foreach(var file in Files)
        {
            string path = Path.Combine(Application.persistentDataPath, file);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted {path}");
            }
        }
    }
}
