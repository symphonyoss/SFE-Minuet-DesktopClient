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
using System.Windows.Forms;
using System.Windows.Input;

namespace Paragon.Runtime.Kernel.HotKeys
{
    public class HotKeyEventArgs : EventArgs
    {
        public HotKeyEventArgs(string name, ModifierKeys modifiers, Keys keys)
        {
            Name = name;
            Modifiers = modifiers;
            Keys = keys;
        }

        public string Name { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Keys Keys { get; set; }
    }
}