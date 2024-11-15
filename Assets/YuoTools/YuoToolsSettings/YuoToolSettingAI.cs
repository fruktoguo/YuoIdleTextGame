using System;
using YuoTools;

public enum AIServerType
{
    豆包 = 0,
    智谱 = 1
}

[Serializable]
public class AIServerSettings
{
    public string APIKey;
    public string Model;
}

[Serializable]
public class AISetting
{
    public AIServerType Server = AIServerType.豆包;
    public string APIKey => GetServer().APIKey;
    public string Model => GetServer().Model;

    public SerializableDictionary<string, AIServerSettings> Servers = new();

    public AIServerSettings GetServer()
    {
        return GetServer(Server.ToString());
    }

    public AIServerSettings GetServer(string key)
    {
        if (!Servers.ContainsKey(key))
        {
            Servers[key] = new();
        }

        return Servers[key];
    }
}