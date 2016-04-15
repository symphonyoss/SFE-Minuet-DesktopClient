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
using System.Threading;
using System.Threading.Tasks;

namespace Paragon.Plugins.Notifications
{
    public class TaskUtilities
    {
        public static DelayedTask Delay(TimeSpan timeSpan)
        {
            return new DelayedTask(timeSpan);
        }

        public static DelayedTask Delay(int milliseconds)
        {
            return new DelayedTask(TimeSpan.FromMilliseconds(milliseconds));
        }

        public class DelayedTask
        {
            private readonly TimeSpan timeSpan;

            internal DelayedTask(TimeSpan timeSpan)
            {
                this.timeSpan = timeSpan;
            }

            public void ContinueWith(Action action, TaskScheduler taskScheduler)
            {
                Timer timer = null;
                timer = new Timer(ignore =>
                {
                    timer.Dispose();
                    Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                }, null, timeSpan, TimeSpan.FromMilliseconds(-1));
            }
        }
    }
}