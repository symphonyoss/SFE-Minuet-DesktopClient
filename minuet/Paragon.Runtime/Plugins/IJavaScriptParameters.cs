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

using System.Reflection;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Defines translation between parameters received from V8 or remotely, and the C# parameters for the plugin method being invoked.
    /// </summary>
    public interface IJavaScriptParameters
    {
        /// <summary>
        /// Convert the parameter values from the caller into native C# objects that match the supplied parameter definitions.
        /// </summary>
        /// <param name="parameterInfos">
        /// The parameter definitions for the method to be invoked.
        /// </param>
        /// <param name="pluginManager">
        /// The plugin manager used to lookup dynamic plugins passed in as function arguments
        /// </param>
        /// <returns>
        /// Native C# objects matching <param name="parameterInfos"></param>
        /// </returns>
        /// <remarks>
        /// The method will be called by <see cref="JavaScriptPlugin"/> on the calling thread, *before* dispatching to a background thread for execution.
        /// </remarks>
        object[] GetConvertedParameters(ParameterInfo[] parameterInfos, IPluginManager pluginManager);
    }
}