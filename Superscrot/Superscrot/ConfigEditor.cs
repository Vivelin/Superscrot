using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Provides a quick and dirty configuration editor using a property grid.
    /// </summary>
    public partial class ConfigEditor : Form
    {
        private Configuration _localConfig = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Superscrot.ConfigEditor"/> class.
        /// </summary>
        public ConfigEditor()
        {
            InitializeComponent();
        }

        private void ConfigEditor_Load(object sender, EventArgs e)
        {
            _localConfig = Configuration.LoadSettings(Program.SettingsPath);
            propertyGrid.SelectedObject = _localConfig;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _localConfig.SaveSettings(Program.SettingsPath);
            Program.Config = _localConfig;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            _localConfig = Configuration.LoadSettings(Program.SettingsPath);
            propertyGrid.SelectedObject = _localConfig;
        }
    }
}
