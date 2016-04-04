//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

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