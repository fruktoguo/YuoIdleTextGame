using UnityEngine;
using YuoTools.Extend.Helper;

public class DataHelper
{
    static string dataPath = Application.dataPath + "/Data/";

    public static string SavePah = dataPath + "/Save/";

    public static string ConfigPath = dataPath + "/Config/";

    public static string LoadPlayerSave(string saveName)
    {
        FileHelper.CheckOrCreateFile(SavePah + $"{saveName}.json");
        return FileHelper.ReadAllText(SavePah + $"{saveName}.json");
    }

    public static string SavePlayerSave(string saveName, string data)
    {
        FileHelper.CheckOrCreateFile(SavePah + $"{saveName}.json");
        FileHelper.WriteAllText(SavePah + $"{saveName}.json", data);
        return data;
    }

    public static string LoadConfig(string configName)
    {
        FileHelper.CheckOrCreateFile(ConfigPath + $"{configName}.json");
        return FileHelper.ReadAllText(ConfigPath + $"{configName}.json");
    }

    public static void Init()
    {
        PropertyHelper.Init();
    }
}