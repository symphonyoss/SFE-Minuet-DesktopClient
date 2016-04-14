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

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines a callback which can be wrapped in <see cref="JavaScriptPluginCallback"/> and passed to a native plugin object method.
    /// </summary>
    public interface IJavaScriptParameterCallback
    {
        /// <summary>
        /// Invoked by a native plugin object which sees this callback as a <see cref="JavaScriptPluginCallback"/>.
        /// </summary>
        /// <param name="data">The result of the method (assuming it didn't throw an exception), or the payload of the event.</param>
        void Invoke(params object[] data);
    }
}