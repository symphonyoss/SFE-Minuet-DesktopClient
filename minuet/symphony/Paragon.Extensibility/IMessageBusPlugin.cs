﻿//Licensed to the Apache Software Foundation(ASF) under one
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

namespace Paragon.Plugins
{
    public interface IMessageBusPlugin
    {
        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnConnected;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnDisconnected;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnError;

        [JavaScriptPluginMember]
        event JavaScriptPluginCallback OnMessage;

        [JavaScriptPluginMember]
        void PublishMessage(string topic, object message);

        [JavaScriptPluginMember]
        void SendMessage(string topic, object message, string rid);

        [JavaScriptPluginMember]
        void Subscribe(string topic, string rid);

        [JavaScriptPluginMember]
        void Unsubscribe(string topic, string rid);
    }
}