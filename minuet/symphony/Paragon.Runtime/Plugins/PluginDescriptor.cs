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

using System.Collections.Generic;

namespace Paragon.Runtime.Plugins
{
    public enum PluginProcess
    {
        Browser,
        Renderer
    }

    /// <summary>
    /// Defines the metadata required to interact with a plugin, locally or remotely.
    /// </summary>
    public class PluginDescriptor
    {
        /// <summary>
        /// For a static plugin this is the full JavaScript path for the object. For a dynamic plugin it is a Guid.
        /// </summary>
        public string PluginId { get; set; }

        public List<MethodDescriptor> Methods { get; set; }

        public List<string> Events { get; set; }
    }

    public class MethodDescriptor
    {
        public string MethodName { get; set; }
        public bool HasCallbackParameter { get; set; }
        public bool IsVoid { get; set; }
    }
}