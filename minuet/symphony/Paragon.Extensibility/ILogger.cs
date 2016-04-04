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
using System.Diagnostics;

namespace Paragon.Plugins
{
    public delegate void FormatMessageCallback(MessageFormatter formatter);

    public delegate string MessageFormatter(string format, params object[] args);

    public interface ILogger
    {
        void Debug(string message, string caller = null);
        void Debug(FormatMessageCallback formatter, string caller = null);
        void Debug(string format, params object[] args);
        void Info(string message, string caller = null);
        void Info(FormatMessageCallback formatter, string caller = null);
        void Info(string format, params object[] args);
        void Warn(string message, string caller = null);
        void Warn(FormatMessageCallback formatter, string caller = null);
        void Warn(string format, params object[] args);
        void Error(string message, string caller = null);
        void Error(string message, Exception exception, string caller = null);
        void Error(FormatMessageCallback formatter, string caller = null);
        void Error(FormatMessageCallback formatter, Exception exception, string caller = null);
        void Error(string format, params object[] args);
        void Fatal(string message, string caller = null);
        void Fatal(FormatMessageCallback formatter, string caller = null);
        void Fatal(string format, params object[] args);
        SourceLevels Level { get; set; }
    }
}