namespace YuoTools.Extend.AI
{
    public static class AIHelper
    {
        public static string ApiKey => YuoToolsSettings.GetOrCreateSettings().AIApiKey;
    }
}