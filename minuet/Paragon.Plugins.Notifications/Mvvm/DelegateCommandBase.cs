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
using System.Windows.Input;

namespace Paragon.Plugins.Notifications.Mvvm
{
    public abstract class DelegateCommandBase : ICommand
    {
        private readonly Action executeMethod;
        private EventHandler canExecuteChanged;

        protected DelegateCommandBase(Action executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CanExecuteChanged += value; }
            remove { CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            executeMethod();
        }

        protected void Execute(object parameter)
        {
            executeMethod();
        }

        private event EventHandler CanExecuteChanged
        {
            add
            {
                var eventHandler = canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref canExecuteChanged, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                var eventHandler = canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref canExecuteChanged, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }
    }
}