using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using VRC.Core;
using System.IO;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace AvatarLoger
{
    
    public class jews : MelonLoader.MelonMod
    {
        static string PublicAvatarFile = "AvatarLog\\Public.txt";
        static string PrivateAvatarFile = "AvatarLog\\Private.txt";
        static string AvatarIDs = "";
        static Queue<ApiAvatar> AvatarToPost = new Queue<ApiAvatar>();
        static HttpClient WebHookClient = new HttpClient();
        static Config config { get; set; }
        private static HarmonyMethod GetPatch(string name) => new HarmonyMethod(typeof(jews).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        public override void OnApplicationStart()
        {

            Directory.CreateDirectory("AvatarLog");
            if (!File.Exists(PublicAvatarFile))
                File.AppendAllText(PublicAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");
            if (!File.Exists(PrivateAvatarFile))
                File.AppendAllText(PrivateAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");

            foreach (string line in File.ReadAllLines(PublicAvatarFile)) 
                if (line.Contains("Avatar ID"))
                    AvatarIDs += line.Replace("Avatar ID:", "");
            foreach (string line in File.ReadAllLines(PrivateAvatarFile))
                if (line.Contains("Avatar ID"))
                    AvatarIDs += line.Replace("Avatar ID:", "");

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
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("AvatarLog\\Config.json"));
            }

            
            HarmonyInstance patchman = HarmonyInstance.Create("pog");
            patchman.Patch(typeof(AssetBundleDownloadManager).GetMethods().Where(mi => mi.GetParameters().Length == 1 && mi.GetParameters().First().ParameterType == typeof(ApiAvatar) && mi.ReturnType == typeof(void)).FirstOrDefault(), GetPatch("apiavatardownloadthingy"));

            MelonLoader.MelonCoroutines.Start(DoCheck());
        }
        private static bool apiavatardownloadthingy(ApiAvatar __0) 
        {
            if (!AvatarIDs.Contains(__0.id))
            {
                if (__0.releaseStatus == "public")
                {
                    AvatarIDs += __0.id;
                    StringBuilder avatarlog = new StringBuilder();
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
                    if (!string.IsNullOrEmpty(config.PublicWebhook) && CanPost(__0.authorId))
                        AvatarToPost.Enqueue(__0);
                }
                else
                {
                    AvatarIDs += __0.id;
                    StringBuilder avatarlog = new StringBuilder();
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
                    File.AppendAllText(PrivateAvatarFile, avatarlog.ToString());
                    if (!string.IsNullOrEmpty(config.PrivateWebhook) && CanPost(__0.authorId))
                        AvatarToPost.Enqueue(__0);
                }
            }
            return true;
        }
        static bool CanPost(string id)
        {
            if (!config.CanPostSelfAvatar && APIUser.CurrentUser.id.Equals(id))
                return false;
            if (config.CanPostFriendsAvatar)
                return true;
            return !APIUser.CurrentUser.friendIDs.Contains(id);
        }
        IEnumerator DoCheck()
        {
            for (; ; )
            {
                try
                {
                    if (AvatarToPost.Count != 0)
                    {
                        ApiAvatar avatar = AvatarToPost.Peek();
                        AvatarToPost.Dequeue();
                        DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();
                        discordEmbed.WithAuthor(string.IsNullOrEmpty(config.BotName) ? "Loggy boi" : config.BotName, string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL, string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL);
                        discordEmbed.WithImageUrl(avatar.thumbnailImageUrl);
                        discordEmbed.WithColor(new DiscordColor(avatar.releaseStatus == "public" ? "#00FF00" : "#FF0000"));
                        discordEmbed.WithUrl($"https://vrchat.com/api/1/avatars/{avatar.id}?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");
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
                        discordEmbed.WithFooter("Made by KeafyIsHere", string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL);
                        RestWebhookExecutePayload webhookpayload = new RestWebhookExecutePayload
                        {
                            Content = "",
                            Username = string.IsNullOrEmpty(config.BotName) ? "Loggy boi" : config.BotName,
                            AvatarUrl = string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL,
                            IsTTS = false,
                            Embeds = new List<DiscordEmbed>() { discordEmbed.Build() }
                        };
                        WebHookClient.PostAsync(avatar.releaseStatus == "public" ? config.PublicWebhook : config.PrivateWebhook, new StringContent(JsonConvert.SerializeObject(webhookpayload), Encoding.UTF8, "application/json"));
                    }
                }
                catch (Exception ex) 
                {
                    MelonLoader.MelonLogger.Error(ex);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
