using System;
using System.Drawing;
using System.Windows.Forms;

namespace Paragon.Runtime.WinForms
{
    internal class PromptDialog : Form
    {
        private Button _cancelBtn;
        private TextBox _messageText;
        private Button _okBtn;
        private Label _promptText;

        private PromptDialog()
        {
            InitializeComponent();
        }

        public static DialogResult Prompt(IWin32Window owner, string promptText, ref string messageText)
        {
            var d = new PromptDialog
            {
                _promptText = {Text = promptText},
                _messageText = {Text = messageText},
                DialogResult = DialogResult.Cancel
            };

            var r = d.ShowDialog(owner);
            if (r == DialogResult.OK)
            {
                messageText = d._messageText.Text;
            }
            return r;
        }

        private void InitializeComponent()
        {
            _promptText = new Label();
            _messageText = new TextBox();
            _okBtn = new Button();
            _cancelBtn = new Button();
            SuspendLayout();
            // 
            // promptText
            // 
            _promptText.AutoSize = true;
            _promptText.Location = new Point(13, 13);
            _promptText.Name = "_promptText";
            _promptText.Size = new Size(0, 13);
            _promptText.TabIndex = 0;
            // 
            // messageText
            // 
            _messageText.Location = new Point(16, 37);
            _messageText.Name = "_messageText";
            _messageText.Size = new Size(291, 20);
            _messageText.TabIndex = 1;
            // 
            // okBtn
            // 
            _okBtn.DialogResult = DialogResult.OK;
            _okBtn.Location = new Point(151, 74);
            _okBtn.Name = "_okBtn";
            _okBtn.Size = new Size(75, 23);
            _okBtn.TabIndex = 2;
            _okBtn.Text = "OK";
            _okBtn.UseVisualStyleBackColor = true;
            _okBtn.Click += OnButtonClick;
            // 
            // cancelBtn
            // 
            _cancelBtn.DialogResult = DialogResult.Cancel;
            _cancelBtn.Location = new Point(232, 74);
            _cancelBtn.Name = "_cancelBtn";
            _cancelBtn.Size = new Size(75, 23);
            _cancelBtn.TabIndex = 2;
            _cancelBtn.Text = "Cancel";
            _cancelBtn.UseVisualStyleBackColor = true;
            _cancelBtn.Click += OnButtonClick;
            // 
            // Form1
            // 
            AcceptButton = _okBtn;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _cancelBtn;
            ClientSize = new Size(319, 109);
            Controls.Add(_cancelBtn);
            Controls.Add(_okBtn);
            Controls.Add(_messageText);
            Controls.Add(_promptText);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "Form1";
            Text = "Prompt";
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            DialogResult = ((Button) sender).DialogResult;
            Close();
        }
    }
}