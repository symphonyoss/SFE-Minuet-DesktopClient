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

using Paragon.Plugins;

// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Kernel.Windowing
{
    public class FrameOptions
    {
        public FrameOptions()
        {
            Icon = true;
            MinimizeButton = true;
            MaximizeButton = true;
        }

        /// <summary>
        /// none, paragon, custom (defaults to paragon)
        /// </summary>
        public FrameType Type { get; set; }

        public bool Icon { get; set; }

        public bool MinimizeButton { get; set; }

        public bool MaximizeButton { get; set; }

        public SystemMenuOptions SystemMenu { get; set; }
    }
}