using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Superscrot.Dialogs
{
    /// <summary>
    /// Represents a dialog that allows users to change the application's settings.
    /// </summary>
    public partial class Settings : Form
    {
        private Configuration configuration;
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> dialog.
        /// </summary>
        public Settings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="Superscrot.Configuration"/> whose 
        /// properties are presented on the form.
        /// </summary>
        public Configuration Configuration
        {
            get { return configuration; }
            set
            {
                if (value != configuration)
                {
                    configuration = value;
                    UpdateForm();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether or not changes to the 
        /// current configuration have been applied 
        /// </summary>
        protected bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                applyButton.Enabled = value;
            }
        }

        /// <summary>
        /// Updates the form with data from the current configuration.
        /// </summary>
        protected void UpdateForm()
        {
            // Connection
            // Server
            addressText.Text = Configuration.FtpHostname;
            portNud.Value = Configuration.FtpPort;
            protocolDropdown.SelectedIndex = (Configuration.UseSSH ? 1 : 0);

            // Authentication
            usernameText.Text = Configuration.FtpUsername;
            passwordText.Text = Configuration.FtpPassword;
            keyText.Text = Configuration.PrivateKeyPath;
            fingerprintText.Text = configuration.HostKeyFingerprint;

            // WinSCP
            scpText.Text = Configuration.WinScpPath;

            IsDirty = false;
        }

        /// <summary>
        /// Updates the current configuration with input from the form.
        /// </summary>
        protected void UpdateConfigation()
        {
            // Connection
            // Server
            Configuration.FtpHostname = addressText.Text;
            Configuration.FtpPort = (int)portNud.Value;
            Configuration.UseSSH = (protocolDropdown.SelectedIndex == 1);

            // Authentication
            Configuration.FtpUsername = usernameText.Text;
            Configuration.FtpPassword = passwordText.Text;
            Configuration.PrivateKeyPath = keyText.Text;
            Configuration.HostKeyFingerprint = fingerprintText.Text;

            // WinSCP
            Configuration.WinScpPath = scpText.Text;

            IsDirty = false;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            UpdateConfigation();
            Program.Config = Configuration;
            Program.Config.SaveSettings(Program.SettingsPath);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            applyButton_Click(sender, e);
            Close();
        }

        #region Interaction between form controls
        private void addressText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void portNud_ValueChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void protocolDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsDirty = true;

            // Enable/disable inputs depending on their relevance
            var useSsh = (protocolDropdown.SelectedIndex == 1);
            var useScp = System.IO.File.Exists(scpText.Text);
            keyLabel.Enabled = keyText.Enabled = browseKeyButton.Enabled = useSsh && useScp;
            fingerprintLabel.Enabled = fingerprintText.Enabled = useSsh && useScp;
            passwordLabel.Enabled = passwordText.Enabled = !useSsh || !useScp;
            scpGroup.Enabled = useSsh;
        }

        private void usernameText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void passwordText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void keyText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void fingerprintText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void scpText_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;

            /* Enable/disable PrivateKeyPath, HostKeyFingerprint, FtpPassword
             * inputs depending on whether or not they are relevant with WinSCP
             */
            var isValid = System.IO.File.Exists(scpText.Text);
            keyLabel.Enabled = keyText.Enabled = browseKeyButton.Enabled = isValid;
            fingerprintLabel.Enabled = fingerprintText.Enabled = isValid;
            passwordLabel.Enabled = passwordText.Enabled = !isValid;
        }
        #endregion
    }
}
