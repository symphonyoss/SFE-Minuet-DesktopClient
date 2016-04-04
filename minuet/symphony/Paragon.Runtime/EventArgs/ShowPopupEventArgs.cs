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

namespace Paragon.Runtime
{
    public class ShowPopupEventArgs : EventArgs
    {
        public ShowPopupEventArgs(ICefWebBrowser browser)
        {
            PopupBrowser = browser;
            Shown = false;
        }

        /// <summary>
        /// The new popup browser control. The handler must show this in a custom frame window.
        /// </summary>
        public ICefWebBrowser PopupBrowser { get; private set; }

        /// <summary>
        /// The handler must set this property true. If this property is not set to true, the browser control will be immediately destroyed.
        /// </summary>
        public bool Shown { get; set; }
    }
}