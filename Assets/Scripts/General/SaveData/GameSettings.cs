using System.IO;
using UnityEngine;

public static class GameSettings
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "GameSettings.json");

    public static void Save(GameSettingsData data)
    {
        string json = JsonUtility.ToJson(data,true);
        File.WriteAllText(FilePath, json);
    }

    public static GameSettingsData Load()
    {
        if (!File.Exists(FilePath))
        {
            return CreateDefault();
        }

        string json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<GameSettingsData>(json);
        return data ?? CreateDefault();
    }

    private static GameSettingsData CreateDefault()
    {
        GameSettingsData data = new GameSettingsData();

        data.masterVolume = 0.2f;
        data.musicVolume = 1f;
        data.soundsVolume = 1f;
        
        data.resolution = Screen.currentResolution;
        data.windowType = FullScreenMode.FullScreenWindow;
        
        data.sensitivity = 0.15f;
        data.fov = 90;

        data.crosshairIndex = 0;
        data.crosshairColor = new Color(1f,1f,1f,1f);
        data.crosshairScale = new Vector3(0.5f,0.5f,0.5f);
        data.crosshairRotation = new Vector3(0f,0f,0f);

        return data;
    }
}
