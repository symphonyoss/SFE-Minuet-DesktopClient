using System;
using Newtonsoft.Json.Linq;
using Paragon.Plugins.Notifications.Contracts;

namespace Paragon.Plugins.Notifications.Client
{
    public static class NotificationOptionsFactory
    {
        public static NotificationOptions Create(string text)
        {
            return Create(JObject.Parse(text));
        }

        public static NotificationOptions Create(JObject json)
        {
            var options = new NotificationOptions();
            options.BackgroundColor = GetValueOrDefault(json, "color", "505050");
            options.BlinkColor = GetValueOrDefault(json, "blinkColor", "ffa500");
            options.CanBlink = GetValueOrDefault(json, "blink", false);
            options.CanPlaySound = GetValueOrDefault(json, "playSound", false);
            options.Callback = GetValueOrDefault<string>(json, "callback", null);
            options.CallbackArg = GetValueOrDefault<string>(json, "callbackArg", null);
            options.Group = GetValueOrDefault(json, "grouping", "default");
            options.IconUrl = GetValueOrDefault<string>(json, "imageUri", null);
            options.IsClickable = true;
            options.IsPersistent = GetValueOrDefault(json, "persistent", false);
            options.Message = GetValueOrDefault<string>(json, "message", null);
            options.SoundFile = GetValueOrDefault<string>(json, "soundFile", null);
            options.Title = GetValueOrDefault<string>(json, "title", null);

            return options;
        }

        private static T GetValueOrDefault<T>(JObject json, string path, T defaultValue)
        {
            var token = json.SelectToken(path);

            try
            {
                return token != null
                    ? token.ToObject<T>()
                    : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}