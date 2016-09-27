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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Windowing
{
    public class CreateWindowOptions
    {
        public CreateWindowOptions()
        {
            Frame = new FrameOptions();
            InitialState = "normal";
            AutoSaveLocation = true;
            Resizable = true;
            Focused = true;
        }

        /// <summary>
        /// Optional. Id to identify the window. This will be used to remember the size and position of the window and restore that geometry when a window with the same id is later opened. If a window with a given id is created while another window with the same id already exists, the currently opened window will be focused instead of creating a new window.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Used to specify the initial position, initial size and constraints of the window (including window decorations such as the title bar and frame). If an id is also specified and a window with a matching id has been shown before, the remembered bounds will be used instead.
        /// Note that the padding between the inner and outer bounds is determined by the OS. Therefore setting the same bounds property for both the innerBounds and outerBounds will result in an error.
        /// </summary>
        public BoundsSpecification OuterBounds { get; set; }

        /// <summary>
        /// For none, the -webkit-app-region CSS property can be used to apply draggability to the app's window. -webkit-app-region: drag can be used to mark regions draggable. no-drag can be used to disable this style on nested elements.
        /// </summary>
        [JsonConverter(typeof (FrameOptionsConverter))]
        public FrameOptions Frame { get; set; }

        /// <summary>
        /// The initial state of the window, allowing it to be created already fullscreen, maximized, or minimized. Defaults to 'normal'.
        /// </summary>
        public string InitialState { get; set; }

        /// <summary>
        /// If true, the window will be created in a hidden state. Call show() on the window to show it once it has been created. Defaults to false.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// If true, the window will be resizable by the user. Defaults to true.
        /// </summary>
        public bool Resizable { get; set; }

        /// <summary>
        /// If true, the window will stay above most other windows. If there are multiple windows of this kind, the currently focused window will be in the foreground. Requires the "alwaysOnTopWindows" permission. Defaults to false.
        /// </summary>
        public bool AlwaysOnTop { get; set; }

        /// <summary>
        /// If true, the window will be focused when created. Defaults to true.
        /// </summary>
        public bool Focused { get; set; }

        /// <summary>
        /// If true, clicking the close button will minimize the window. Defaults to false.
        /// </summary>
        public bool MinimizeOnClose { get; set; }

        /// <summary>
        /// If true, app will launch when Windows starts. Defaults to false.
        /// </summary>
        public bool LaunchOnStartup { get; set; }

        /// <summary>
        /// If true, enables the support of hot keys for this window. Defaults to false.
        /// </summary>
        public bool HotKeysEnabled { get; set; }

        /// <summary>
        /// If true, enables the application to automatically remember the location of the window and restore it in the last know position.
        /// </summary>
        public bool AutoSaveLocation { get; set; }
    }

    /// <summary>
    /// Provides deserialization of the 'frame' property in <see cref="CreateWindowOptions"/>, whose value can be a string or an object.
    /// </summary>
    public class FrameOptionsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonObject = JObject.FromObject(value);
            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                FrameType frameType;
                var frameTypeName = reader.Value as string;
                if (string.IsNullOrEmpty(frameTypeName))
                {
                    frameType = FrameType.Paragon;
                }
                else
                {
                    try
                    {
                        frameType = (FrameType) Enum.Parse(typeof (FrameType), frameTypeName, true);
                    }
                    catch
                    {
                        frameType = FrameType.Paragon;
                    }
                }

                return new FrameOptions {Type = frameType};
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JToken.ReadFrom(reader);
                return o.ToObject<FrameOptions>(serializer);
            }
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (FrameOptions);
        }
    }
}