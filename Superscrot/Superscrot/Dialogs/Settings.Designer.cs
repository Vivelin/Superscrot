namespace Superscrot.Dialogs
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.serverGroup = new System.Windows.Forms.GroupBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.addressText = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.portNud = new System.Windows.Forms.NumericUpDown();
            this.protocolLabel = new System.Windows.Forms.Label();
            this.protocolDropdown = new System.Windows.Forms.ComboBox();
            this.authGroup = new System.Windows.Forms.GroupBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameText = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordText = new System.Windows.Forms.TextBox();
            this.keyLabel = new System.Windows.Forms.Label();
            this.keyText = new System.Windows.Forms.TextBox();
            this.browseKeyButton = new System.Windows.Forms.Button();
            this.fingerprintLabel = new System.Windows.Forms.Label();
            this.fingerprintText = new System.Windows.Forms.TextBox();
            this.scpGroup = new System.Windows.Forms.GroupBox();
            this.scpLabel = new System.Windows.Forms.Label();
            this.scpText = new System.Windows.Forms.TextBox();
            this.browseScpButton = new System.Windows.Forms.Button();
            this.scpDescriptionLabel = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.serverGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNud)).BeginInit();
            this.authGroup.SuspendLayout();
            this.scpGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(441, 507);
            this.tabControl.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.scpGroup);
            this.tabPage2.Controls.Add(this.authGroup);
            this.tabPage2.Controls.Add(this.serverGroup);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(433, 479);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Connection";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(212, 525);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(293, 525);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(374, 525);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 1;
            this.applyButton.Text = "&Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // serverGroup
            // 
            this.serverGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serverGroup.Controls.Add(this.protocolDropdown);
            this.serverGroup.Controls.Add(this.protocolLabel);
            this.serverGroup.Controls.Add(this.portNud);
            this.serverGroup.Controls.Add(this.portLabel);
            this.serverGroup.Controls.Add(this.addressText);
            this.serverGroup.Controls.Add(this.addressLabel);
            this.serverGroup.Location = new System.Drawing.Point(6, 6);
            this.serverGroup.Name = "serverGroup";
            this.serverGroup.Size = new System.Drawing.Size(421, 119);
            this.serverGroup.TabIndex = 0;
            this.serverGroup.TabStop = false;
            this.serverGroup.Text = "Server";
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(6, 25);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(52, 15);
            this.addressLabel.TabIndex = 0;
            this.addressLabel.Text = "&Address:";
            // 
            // addressText
            // 
            this.addressText.Location = new System.Drawing.Point(75, 22);
            this.addressText.Name = "addressText";
            this.addressText.Size = new System.Drawing.Size(203, 23);
            this.addressText.TabIndex = 1;
            this.addressText.TextChanged += new System.EventHandler(this.addressText_TextChanged);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(6, 53);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(32, 15);
            this.portLabel.TabIndex = 2;
            this.portLabel.Text = "Po&rt:";
            // 
            // portNud
            // 
            this.portNud.Location = new System.Drawing.Point(75, 51);
            this.portNud.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portNud.Name = "portNud";
            this.portNud.Size = new System.Drawing.Size(120, 23);
            this.portNud.TabIndex = 3;
            this.portNud.ValueChanged += new System.EventHandler(this.portNud_ValueChanged);
            // 
            // protocolLabel
            // 
            this.protocolLabel.AutoSize = true;
            this.protocolLabel.Location = new System.Drawing.Point(6, 83);
            this.protocolLabel.Name = "protocolLabel";
            this.protocolLabel.Size = new System.Drawing.Size(55, 15);
            this.protocolLabel.TabIndex = 4;
            this.protocolLabel.Text = "Pr&otocol:";
            // 
            // protocolDropdown
            // 
            this.protocolDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolDropdown.FormattingEnabled = true;
            this.protocolDropdown.Items.AddRange(new object[] {
            "FTP",
            "SFTP"});
            this.protocolDropdown.Location = new System.Drawing.Point(75, 80);
            this.protocolDropdown.Name = "protocolDropdown";
            this.protocolDropdown.Size = new System.Drawing.Size(121, 23);
            this.protocolDropdown.TabIndex = 5;
            this.protocolDropdown.SelectedIndexChanged += new System.EventHandler(this.protocolDropdown_SelectedIndexChanged);
            // 
            // authGroup
            // 
            this.authGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authGroup.Controls.Add(this.fingerprintText);
            this.authGroup.Controls.Add(this.fingerprintLabel);
            this.authGroup.Controls.Add(this.browseKeyButton);
            this.authGroup.Controls.Add(this.keyText);
            this.authGroup.Controls.Add(this.keyLabel);
            this.authGroup.Controls.Add(this.passwordText);
            this.authGroup.Controls.Add(this.passwordLabel);
            this.authGroup.Controls.Add(this.usernameText);
            this.authGroup.Controls.Add(this.usernameLabel);
            this.authGroup.Location = new System.Drawing.Point(6, 131);
            this.authGroup.Name = "authGroup";
            this.authGroup.Size = new System.Drawing.Size(421, 206);
            this.authGroup.TabIndex = 1;
            this.authGroup.TabStop = false;
            this.authGroup.Text = "Authentication";
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(6, 25);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(63, 15);
            this.usernameLabel.TabIndex = 0;
            this.usernameLabel.Text = "&Username:";
            // 
            // usernameText
            // 
            this.usernameText.Location = new System.Drawing.Point(75, 22);
            this.usernameText.Name = "usernameText";
            this.usernameText.Size = new System.Drawing.Size(203, 23);
            this.usernameText.TabIndex = 1;
            this.usernameText.TextChanged += new System.EventHandler(this.usernameText_TextChanged);
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(6, 54);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(60, 15);
            this.passwordLabel.TabIndex = 2;
            this.passwordLabel.Text = "&Password:";
            // 
            // passwordText
            // 
            this.passwordText.Location = new System.Drawing.Point(75, 51);
            this.passwordText.Name = "passwordText";
            this.passwordText.Size = new System.Drawing.Size(203, 23);
            this.passwordText.TabIndex = 3;
            this.passwordText.UseSystemPasswordChar = true;
            this.passwordText.TextChanged += new System.EventHandler(this.passwordText_TextChanged);
            // 
            // keyLabel
            // 
            this.keyLabel.AutoSize = true;
            this.keyLabel.Location = new System.Drawing.Point(6, 92);
            this.keyLabel.Name = "keyLabel";
            this.keyLabel.Size = new System.Drawing.Size(86, 15);
            this.keyLabel.TabIndex = 4;
            this.keyLabel.Text = "Private &key file:";
            // 
            // keyText
            // 
            this.keyText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.keyText.Location = new System.Drawing.Point(9, 110);
            this.keyText.Name = "keyText";
            this.keyText.Size = new System.Drawing.Size(325, 23);
            this.keyText.TabIndex = 5;
            this.keyText.TextChanged += new System.EventHandler(this.keyText_TextChanged);
            // 
            // browseKeyButton
            // 
            this.browseKeyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseKeyButton.Location = new System.Drawing.Point(340, 110);
            this.browseKeyButton.Name = "browseKeyButton";
            this.browseKeyButton.Size = new System.Drawing.Size(75, 23);
            this.browseKeyButton.TabIndex = 6;
            this.browseKeyButton.Text = "&Browse...";
            this.browseKeyButton.UseVisualStyleBackColor = true;
            // 
            // fingerprintLabel
            // 
            this.fingerprintLabel.AutoSize = true;
            this.fingerprintLabel.Location = new System.Drawing.Point(6, 149);
            this.fingerprintLabel.Name = "fingerprintLabel";
            this.fingerprintLabel.Size = new System.Drawing.Size(122, 15);
            this.fingerprintLabel.TabIndex = 7;
            this.fingerprintLabel.Text = "Server key &fingerprint:";
            // 
            // fingerprintText
            // 
            this.fingerprintText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fingerprintText.Location = new System.Drawing.Point(9, 167);
            this.fingerprintText.Name = "fingerprintText";
            this.fingerprintText.Size = new System.Drawing.Size(406, 23);
            this.fingerprintText.TabIndex = 8;
            this.fingerprintText.TextChanged += new System.EventHandler(this.fingerprintText_TextChanged);
            // 
            // scpGroup
            // 
            this.scpGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scpGroup.Controls.Add(this.scpDescriptionLabel);
            this.scpGroup.Controls.Add(this.browseScpButton);
            this.scpGroup.Controls.Add(this.scpText);
            this.scpGroup.Controls.Add(this.scpLabel);
            this.scpGroup.Location = new System.Drawing.Point(6, 343);
            this.scpGroup.Name = "scpGroup";
            this.scpGroup.Size = new System.Drawing.Size(421, 129);
            this.scpGroup.TabIndex = 2;
            this.scpGroup.TabStop = false;
            this.scpGroup.Text = "WinSCP";
            // 
            // scpLabel
            // 
            this.scpLabel.AutoSize = true;
            this.scpLabel.Location = new System.Drawing.Point(6, 76);
            this.scpLabel.Name = "scpLabel";
            this.scpLabel.Size = new System.Drawing.Size(56, 15);
            this.scpLabel.TabIndex = 0;
            this.scpLabel.Text = "&Location:";
            // 
            // scpText
            // 
            this.scpText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scpText.Location = new System.Drawing.Point(9, 94);
            this.scpText.Name = "scpText";
            this.scpText.Size = new System.Drawing.Size(325, 23);
            this.scpText.TabIndex = 1;
            this.scpText.TextChanged += new System.EventHandler(this.scpText_TextChanged);
            // 
            // browseScpButton
            // 
            this.browseScpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseScpButton.Location = new System.Drawing.Point(340, 94);
            this.browseScpButton.Name = "browseScpButton";
            this.browseScpButton.Size = new System.Drawing.Size(75, 23);
            this.browseScpButton.TabIndex = 2;
            this.browseScpButton.Text = "Bro&wse...";
            this.browseScpButton.UseVisualStyleBackColor = true;
            // 
            // scpDescriptionLabel
            // 
            this.scpDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scpDescriptionLabel.Location = new System.Drawing.Point(6, 19);
            this.scpDescriptionLabel.Name = "scpDescriptionLabel";
            this.scpDescriptionLabel.Size = new System.Drawing.Size(409, 46);
            this.scpDescriptionLabel.TabIndex = 3;
            this.scpDescriptionLabel.Text = "You need to have WinSCP installed in order to use SSH key authentication. Additio" +
    "nally, Pageant should be running if your private key requires a passphrase.\r\n";
            // 
            // Settings
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(465, 560);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.Text = "Settings";
            this.tabControl.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.serverGroup.ResumeLayout(false);
            this.serverGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNud)).EndInit();
            this.authGroup.ResumeLayout(false);
            this.authGroup.PerformLayout();
            this.scpGroup.ResumeLayout(false);
            this.scpGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox serverGroup;
        private System.Windows.Forms.TextBox addressText;
        private System.Windows.Forms.Label addressLabel;
        private System.Windows.Forms.ComboBox protocolDropdown;
        private System.Windows.Forms.Label protocolLabel;
        private System.Windows.Forms.NumericUpDown portNud;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.GroupBox authGroup;
        private System.Windows.Forms.TextBox passwordText;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox usernameText;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Button browseKeyButton;
        private System.Windows.Forms.TextBox keyText;
        private System.Windows.Forms.Label keyLabel;
        private System.Windows.Forms.TextBox fingerprintText;
        private System.Windows.Forms.Label fingerprintLabel;
        private System.Windows.Forms.GroupBox scpGroup;
        private System.Windows.Forms.Label scpDescriptionLabel;
        private System.Windows.Forms.Button browseScpButton;
        private System.Windows.Forms.TextBox scpText;
        private System.Windows.Forms.Label scpLabel;
    }
}