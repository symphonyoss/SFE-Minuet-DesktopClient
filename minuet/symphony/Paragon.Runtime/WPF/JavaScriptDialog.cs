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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WPF
{
    public static class JavaScriptDialog
    {
        public static void Alert(string title, string message, ParagonWindow owner)
        {
            var dialog = new JavaScriptDialogWindow {CustomChromeEnabled = owner.CustomChromeEnabled};
            Show(title, null, ref message, false, owner, dialog);
        }

        public static void Alert(string title, string message, ContentControl customContent, IntPtr owner)
        {
            Show(title, null, ref message, false, customContent, owner);
        }

        public static MessageBoxResult Confirm(string title, string message, ParagonWindow owner)
        {
            var dialog = new JavaScriptDialogWindow {CustomChromeEnabled = owner.CustomChromeEnabled};
            return Show(title, null, ref message, true, owner, dialog);
        }

        public static MessageBoxResult Confirm(string title, string message, ContentControl customContent, IntPtr owner)
        {
            return Show(title, null, ref message, true, customContent, owner);
        }

        public static MessageBoxResult Prompt(string title, string promptText, ref string message, ParagonWindow owner)
        {
            var dialog = new JavaScriptDialogWindow {CustomChromeEnabled = owner.CustomChromeEnabled};
            return Show(title, promptText, ref message, true, owner, dialog);
        }

        public static MessageBoxResult Prompt(string title, string promptText, ref string message, ContentControl customContent, IntPtr owner)
        {
            return Show(title, promptText, ref message, true, customContent, owner);
        }

        private static MessageBoxResult Show(string title, string promptText, ref string message, bool needCancel, Window owner, ContentControl customContent)
        {
            var ownerHandle = owner != null ? new WindowInteropHelper(owner).EnsureHandle() : IntPtr.Zero;
            return Show(title, promptText, ref message, needCancel, customContent, ownerHandle);
        }

        private static MessageBoxResult Show(string title, string promptText, ref string message, bool needCancel, ContentControl customContent, IntPtr owner)
        {
            var dialogControl = new JavaScriptDialogControl();
            customContent.Content = dialogControl;
            var hostWindow = Window.GetWindow(customContent);
            if (hostWindow == null)
            {
                throw new InvalidOperationException("Unable to resolve the parent window to the dialog control");
            }
            dialogControl.OkButton.Click +=
                (sender, args) =>
                {
                    hostWindow.DialogResult = true;
                    hostWindow.Close();
                };

            dialogControl.CancelButton.Click +=
                (sender, args) =>
                {
                    hostWindow.DialogResult = false;
                    hostWindow.Close();
                };

            dialogControl.CancelButton.Visibility = needCancel
                ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(promptText))
            {
                // Alert or Confirm
                dialogControl.PromptTextLabel.Text = message;
                dialogControl.UserInputBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                dialogControl.PromptTextLabel.Text = promptText;
                dialogControl.UserInputBox.Text = message;
            }

            hostWindow.Title = title;

            var ownerWindow = HwndSource.FromHwnd(owner);
            if (ownerWindow != null)
            {
                var ow = ownerWindow.RootVisual as Window;
                if( ow != null )
                {
                    hostWindow.Owner = ow;
                    hostWindow.Style = ow.Style;
                }
            }
            else
            {
                Win32Api.SetWindowOwner(new WindowInteropHelper(hostWindow).EnsureHandle(), owner);
            }
            hostWindow.ShowDialog();

            message = dialogControl.UserInputBox.Text;

            return hostWindow.DialogResult != null && hostWindow.DialogResult.Value ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }
    }
}