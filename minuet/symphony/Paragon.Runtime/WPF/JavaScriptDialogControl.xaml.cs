using System.Windows.Controls;

namespace Paragon.Runtime.WPF
{
    internal partial class JavaScriptDialogControl
    {
        internal JavaScriptDialogControl()
        {
            InitializeComponent();
        }

        internal TextBlock PromptTextLabel
        {
            get { return _promptTextLabel; }
        }

        internal TextBox UserInputBox
        {
            get { return _userInputBox; }
        }

        internal Button OkButton
        {
            get { return _okButton; }
        }

        internal Button CancelButton
        {
            get { return _cancelButton; }
        }
    }
}