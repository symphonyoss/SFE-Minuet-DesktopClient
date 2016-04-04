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

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines the functionality required for function callbacks and event listeners accepted by <see cref="JavaScriptPlugin"/>.
    /// </summary>
    public interface IJavaScriptPluginCallback
    {
        /// <summary>
        /// A unique ID for the callback.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        /// Invoked by <see cref="JavaScriptPlugin"/> when a function completes or an event fires, depending what the callback was registered with.
        /// </summary>
        /// <param name="data">The result of the method (assuming it didn't throw an exception), or the payload of the event.</param>
        /// <param name="errorCode">Provided for functions when an exception was caught.</param>
        /// <param name="error">Provided for functions when an exception was caught.</param>
        void Invoke(object data, int errorCode, string error);

        /// <summary>
        /// Returns a callback to be supplied as a parameter to method owned by <see cref="JavaScriptPlugin"/> when the 
        /// method expects to be supplied a callback to invoke itself.
        /// </summary>
        IJavaScriptParameterCallback GetParameterCallback();
    }
}