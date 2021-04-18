using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLoger
{
    public class Config
    {
        public string PublicWebhook { get; set; }
        public string PrivateWebhook { get; set; }
        public string BotName { get; set; }
        public string AvatarURL { get; set; }
        public bool CanPostFriendsAvatar { get; set; }
        public bool CanPostSelfAvatar { get; set; }
    }
}
