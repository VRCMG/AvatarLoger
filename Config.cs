namespace AvatarLoger
{
    public class Config
    {
        public string PublicWebhook { get; set; }
        public string PrivateWebhook { get; set; }
        public static string BotName => null;
        public static string AvatarURL => null;
        public bool CanPostFriendsAvatar { get; set; }
        public static bool CanPostSelfAvatar => false;
    }
}