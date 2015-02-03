﻿using System;
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
    /// Represents a dialog that allows to user to choose what action to take 
    /// when a duplicate file is found.
    /// </summary>
    public partial class DuplicateFileFoundDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateFileFoundDialog"/> 
        /// class with the specified screenshot and the name of the duplicate 
        /// file.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> being uploaded.</param>
        /// <param name="fileName">The name of the duplicate file on the server.</param>
        public DuplicateFileFoundDialog(Screenshot screenshot, string fileName)
        {
            InitializeComponent();

            MainInstruction.Text = string.Format(MainInstruction.Text, fileName);
        }

        private void DuplicateFileFoundDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Config.CheckForDuplicateFiles = !PreventDialogCheckbox.Checked;
            Program.Config.SaveSettings(Program.SettingsPath);
        }
    }
}
