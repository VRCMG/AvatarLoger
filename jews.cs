using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using VRC.Core;

namespace AvatarLoger
{
    public class Jews : MelonMod
    {
        private const string PublicAvatarFile = "AvatarLog\\Public.txt";
        private const string PrivateAvatarFile = "AvatarLog\\Private.txt";
        private static string _avatarIDs = "";
        private static readonly Queue<ApiAvatar> AvatarToPost = new Queue<ApiAvatar>();
        private static readonly HttpClient WebHookClient = new HttpClient();
        private static Config Config { get; set; }

        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Jews).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void OnApplicationStart()
        {
            Directory.CreateDirectory("AvatarLog");
            if (!File.Exists(PublicAvatarFile))
                File.AppendAllText(PublicAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");
            if (!File.Exists(PrivateAvatarFile))
                File.AppendAllText(PrivateAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");

            foreach (var line in File.ReadAllLines(PublicAvatarFile))
                if (line.Contains("Avatar ID"))
                    _avatarIDs += line.Replace("Avatar ID:", "");
            foreach (var line in File.ReadAllLines(PrivateAvatarFile))
                if (line.Contains("Avatar ID"))
                    _avatarIDs += line.Replace("Avatar ID:", "");

            if (!File.Exists("AvatarLog\\Config.json"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Config.json not found!");
                Console.WriteLine("Config.json Generating new one please fill out");
                File.WriteAllText("AvatarLog\\Config.json", JsonConvert.SerializeObject(new Config
                {
                    CanPostFriendsAvatar = false,
                    PrivateWebhook = "",
                    PublicWebhook = ""
                }, Formatting.Indented));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Config File Detected!");
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("AvatarLog\\Config.json"));
            }


            var patchMan = HarmonyInstance.Create("pog");
            patchMan.Patch(
                typeof(AssetBundleDownloadManager).GetMethods().FirstOrDefault(mi =>
                    mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType == typeof(ApiAvatar) &&
                    mi.ReturnType == typeof(void)), GetPatch("ApiAvatarDownloadPatch"));

            new Thread(DoCheck).Start();
        }

        private static bool ApiAvatarDownloadPatch(ApiAvatar __0)
        {
            if (!_avatarIDs.Contains(__0.id))
            {
                if (__0.releaseStatus == "public")
                {
                    _avatarIDs += __0.id;
                    var avatarlog = new StringBuilder();
                    avatarlog.AppendLine($"Avatar ID:{__0.id}");
                    avatarlog.AppendLine($"Avatar Name:{__0.name}");
                    avatarlog.AppendLine($"Avatar Description:{__0.description}");
                    avatarlog.AppendLine($"Avatar Author ID:{__0.authorId}");
                    avatarlog.AppendLine($"Avatar Author Name:{__0.authorName}");
                    avatarlog.AppendLine($"Avatar Asset URL:{__0.assetUrl}");
                    avatarlog.AppendLine($"Avatar Image URL:{__0.imageUrl}");
                    avatarlog.AppendLine($"Avatar Thumbnail Image URL:{__0.thumbnailImageUrl}");
                    avatarlog.AppendLine($"Avatar Release Status:{__0.releaseStatus}");
                    avatarlog.AppendLine($"Avatar Version:{__0.version}");
                    avatarlog.AppendLine(Environment.NewLine);
                    File.AppendAllText(PublicAvatarFile, avatarlog.ToString());
                    avatarlog.Clear();
                    if (!string.IsNullOrEmpty(Config.PublicWebhook) && CanPost(__0.authorId))
                        AvatarToPost.Enqueue(__0);
                }
                else
                {
                    _avatarIDs += __0.id;
                    var avatarlog = new StringBuilder();
                    avatarlog.AppendLine($"Avatar ID:{__0.id}");
                    avatarlog.AppendLine($"Avatar Name:{__0.name}");
                    avatarlog.AppendLine($"Avatar Description:{__0.description}");
                    avatarlog.AppendLine($"Avatar Author ID:{__0.authorId}");
                    avatarlog.AppendLine($"Avatar Author Name:{__0.authorName}");
                    avatarlog.AppendLine($"Avatar Asset URL:{__0.assetUrl}");
                    avatarlog.AppendLine($"Avatar Image URL:{__0.imageUrl}");
                    avatarlog.AppendLine($"Avatar Thumbnail Image URL:{__0.thumbnailImageUrl}");
                    avatarlog.AppendLine($"Avatar Release Status:{__0.releaseStatus}");
                    avatarlog.AppendLine($"Avatar Version:{__0.version}");
                    avatarlog.AppendLine(Environment.NewLine);
                    avatarlog.Clear();
                    File.AppendAllText(PrivateAvatarFile, avatarlog.ToString());
                    if (!string.IsNullOrEmpty(Config.PrivateWebhook) && CanPost(__0.authorId))
                        AvatarToPost.Enqueue(__0);
                }
            }

            return true;
        }

        private static bool CanPost(string id)
        {
            if (!Config.CanPostSelfAvatar && APIUser.CurrentUser.id.Equals(id))
                return false;
            if (Config.CanPostFriendsAvatar)
                return true;
            return !APIUser.CurrentUser.friendIDs.Contains(id);
        }

        private static void DoCheck()
        {
            for (;;)
            {
                try
                {
                    if (AvatarToPost.Count != 0)
                    {
                        var avatar = AvatarToPost.Peek();
                        AvatarToPost.Dequeue();
                        var discordEmbed = new DiscordEmbedBuilder();
                        discordEmbed.WithAuthor(string.IsNullOrEmpty(Config.BotName) ? "Loggy boi" : Config.BotName,
                            string.IsNullOrEmpty(Config.AvatarURL)
                                ? "https://i.imgur.com/No3R2yY.jpg"
                                : Config.AvatarURL,
                            string.IsNullOrEmpty(Config.AvatarURL)
                                ? "https://i.imgur.com/No3R2yY.jpg"
                                : Config.AvatarURL);
                        discordEmbed.WithImageUrl(avatar.thumbnailImageUrl);
                        discordEmbed.WithColor(
                            new DiscordColor(avatar.releaseStatus == "public" ? "#00FF00" : "#FF0000"));
                        discordEmbed.WithUrl(
                            $"https://vrchat.com/api/1/avatars/{avatar.id}?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");
                        discordEmbed.WithTitle("Click Me (API Link)");
                        discordEmbed.WithDescription("Must be logged in on VRChat.com to view api link ^^");
                        discordEmbed.WithTimestamp(DateTimeOffset.Now);
                        discordEmbed.AddField("Avatar ID:", avatar.id);
                        discordEmbed.AddField("Avatar Name:", avatar.name);
                        discordEmbed.AddField("Avatar Description:", avatar.description);
                        discordEmbed.AddField("Avatar Author ID:", avatar.authorId);
                        discordEmbed.AddField("Avatar Author Name:", avatar.authorName);
                        discordEmbed.AddField("Avatar Version:", avatar.version.ToString());
                        discordEmbed.AddField("Avatar Release Status:", avatar.releaseStatus);
                        discordEmbed.AddField("Avatar Asset URL:", avatar.assetUrl);
                        discordEmbed.AddField("Avatar Image URL:", avatar.imageUrl);
                        discordEmbed.AddField("Avatar Thumbnail Image URL:", avatar.thumbnailImageUrl);
                        discordEmbed.WithFooter("Made by KeafyIsHere",
                            string.IsNullOrEmpty(Config.AvatarURL)
                                ? "https://i.imgur.com/No3R2yY.jpg"
                                : Config.AvatarURL);
                        var restWebhookPayload = new RestWebhookExecutePayload
                        {
                            Content = "",
                            Username = string.IsNullOrEmpty(Config.BotName) ? "Loggy boi" : Config.BotName,
                            AvatarUrl = string.IsNullOrEmpty(Config.AvatarURL)
                                ? "https://i.imgur.com/No3R2yY.jpg"
                                : Config.AvatarURL,
                            IsTTS = false,
                            Embeds = new List<DiscordEmbed> {discordEmbed.Build()}
                        };
                        WebHookClient.PostAsync(
                            avatar.releaseStatus == "public" ? Config.PublicWebhook : Config.PrivateWebhook,
                            new StringContent(JsonConvert.SerializeObject(restWebhookPayload), Encoding.UTF8,
                                "application/json"));
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }

                Thread.Sleep(1000);
            }
        }
    }
}