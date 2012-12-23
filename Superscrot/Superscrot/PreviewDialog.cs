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
    public partial class PreviewDialog : Form
    {
        private Screenshot _screenshot;

        public string FileName
        {
            get { return FileNameInput.Text; }
            set { FileNameInput.Text = value; }
        }

        public PreviewDialog(Screenshot s)
        {
            _screenshot = s;

            InitializeComponent();
            FileNameInput.SetCue("Enter a filename...");
        }

        private void UploadDialog_Load(object sender, EventArgs e)
        {
            if (_screenshot != null)
            {
                ScreenshotPreview.Image = _screenshot.Bitmap;
                FileNameInput.Text = _screenshot.GetFileName();
                FileNameInput.Focus();
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            if (DontShowAgain.Checked)
            {
                Program.Config.ShowPreviewDialog = false;
                Program.Config.SaveSettings(Program.SettingsPath);
            }
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                sfd.FileName = FileName.Replace("/", "-").Replace("\\", "-");
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _screenshot.Bitmap.Save(sfd.FileName);
                }
            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(_screenshot.Bitmap);
        }

        private void FileNameInput_TextChanged(object sender, EventArgs e)
        {
            PublicUrl.Text = Common.UriCombine(Program.Config.HttpBaseUri, Common.UrlEncode(FileName));
        }

        private void PublicUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Diagnostics.Process.Start(PublicUrl.Text);
            }
        }
    }
}
