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

namespace Paragon.Plugins.MessageBus
{
    public interface IMessageBroker
    {
        event Action Connected;
        event Action Disconnected;
        event Action<Exception> Error;
        event Action<string, object> MessageReceived;

        void Connect();
        void Disconnect();
        string GetState(string topic);
        void PublishMessage(string topic, object msg);
        void SendMessage(string address, object msg, string responseId);
        void Subscribe(string topic, string responseId);
        void Unsubscribe(string topic, string responseId);
    }
}