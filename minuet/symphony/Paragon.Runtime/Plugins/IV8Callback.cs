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
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Decouples the ability to call back into V8 in response to a function call or event raised.
    /// 
    /// The implementation of this interface handles the type conversion to the appropriate V8 value hierarchy.
    /// </summary>
    public interface IV8Callback : IDisposable
    {
        Guid Identifier { get; }

        void Invoke(IV8PluginRouter router, CefV8Context context, object result, int errorCode, string error);
    }
}