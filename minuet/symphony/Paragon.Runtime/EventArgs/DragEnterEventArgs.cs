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

using System.ComponentModel;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    /// <summary>
    /// Provides the data for a drag enter event that has occured in a CEF browser.
    /// </summary>
    public class DragEnterEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Creates a new <see cref="DragEnterEventArgs"/> with Cancel set to false as default.
        /// </summary>
        /// <param name="dragData"></param>
        /// <param name="mask"></param>
        public DragEnterEventArgs(CefDragData dragData, CefDragOperationsMask mask)
            : base(false)
        {
            Data = dragData;
            Effect = mask;
        }

        /// <summary>
        /// Gets the data associated with the event.
        /// </summary>
        public CefDragData Data { get; private set; }

        /// <summary>
        /// The target drop effect.
        /// </summary>
        public CefDragOperationsMask Effect { get; private set; }
    }
}