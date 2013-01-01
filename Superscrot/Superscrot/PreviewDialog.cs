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
    /// Shows a preview of a screenshot, and allows the user to change the filename, as well as to 
    /// save the screenshot to a file or copy it to the clipboard.
    /// </summary>
    public partial class PreviewDialog : Form
    {
        private Screenshot _screenshot;

        /// <summary>
        /// Gets or sets the filename on the form.
        /// </summary>
        public string FileName
        {
            get { return FileNameInput.Text; }
            set { FileNameInput.Text = value; }
        }

        /// <summary>
        /// Initializes a new instance of the preview dialog for the specified screenshot.
        /// </summary>
        /// <param name="s">The screenshot to preview.</param>
        public PreviewDialog(Screenshot s)
        {
            _screenshot = s;

            InitializeComponent();
            FileNameInput.SetCue("Enter a filename");
        }

        /// <summary>
        /// Displays the screenshot and filename when the form loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadDialog_Load(object sender, EventArgs e)
        {
            if (_screenshot != null)
            {
                ScreenshotPreview.Image = _screenshot.Bitmap;

                string defaultFileName = _screenshot.GetFileName();
                FileNameInput.Text = defaultFileName;

                // Select only the filename itself
                string fileName = System.IO.Path.GetFileNameWithoutExtension(defaultFileName);
                int iStart = defaultFileName.IndexOf(fileName);
                if (iStart < 0)
                    iStart = 0;

                FileNameInput.Select(iStart, fileName.Length);
                FileNameInput.Focus();
            }
        }

        /// <summary>
        /// Saves the "Don't show this dialog again" setting and closes the form when the Upload button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadButton_Click(object sender, EventArgs e)
        {
            if (DontShowAgain.Checked)
            {
                Program.Config.ShowPreviewDialog = false;
                Program.Config.SaveSettings(Program.SettingsPath);
            }
            Close();
        }

        /// <summary>
        /// Closes the form when the cancel button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Prompt the user to save the file when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Copies the image to the clipboard when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(_screenshot.Bitmap);
        }

        /// <summary>
        /// Updates the public link when the filename is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileNameInput_TextChanged(object sender, EventArgs e)
        {
            UploadButton.Enabled = !string.IsNullOrEmpty(FileName);
            if (!string.IsNullOrEmpty(FileName))
            {
                PublicUrl.Text = Common.UriCombine(Program.Config.HttpBaseUri, Common.UrlEncode(FileName));
            }
            else
            {
                PublicUrl.Text = string.Empty;
            }
        }

        /// <summary>
        /// Opens the link when it is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublicUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && PublicUrl.Text.Length > 0)
            {
                System.Diagnostics.Process.Start(PublicUrl.Text);
            }
        }

        /// <summary>
        /// Opens the image in the default image viewer when the preview is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenshotPreview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                string fileName = System.IO.Path.GetTempFileName();
                System.IO.File.Delete(fileName);
                fileName += ".png";

                _screenshot.Bitmap.Save(fileName);
                System.Diagnostics.Process.Start(fileName);
            }
        }
    }
}
