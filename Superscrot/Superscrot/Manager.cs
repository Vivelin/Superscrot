using Superscrot.Uploaders;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace Superscrot
{
    /// <summary>
    /// Coordinates top-level functionality and provides common functions that interact between 
    /// classes. Console output is marked Cyan.
    /// </summary>
    public class Manager : IDisposable
    {
        private KeyboardHook hook = null;
        private History history = null;
        private bool enabled = true;
        private Uploader uploader;

        /// <summary>
        /// Initializes a new instance of the <see cref="Manager"/> class.
        /// </summary>
        public Manager()
        {
            Program.ConfigurationChanged += (sender, e) =>
            {
                if (uploader != null)
                {
                    uploader.Dispose();
                    uploader = null;
                }
            };
        }

        /// <summary>
        /// Occurs when the <see cref="Enabled"/> property changes.
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Manager"/> 
        /// will respond to keyboard input or not.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        /// <summary>
        /// Provides information about taken screenshots.
        /// </summary>
        public History History
        {
            get
            {
                if (history == null)
                {
                    history = new History();
                }
                return history;
            }
        }

        /// <summary>
        /// Initializes the global keyboard hook.
        /// </summary>
        public bool InitializeKeyboardHook()
        {
            try
            {
                hook = new KeyboardHook();
                hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyPressed);
                hook.RegisterHotKey(ModifierKeys.None, Keys.PrintScreen);      //desktop screenshot
                hook.RegisterHotKey(ModifierKeys.Alt, Keys.PrintScreen);       //active window
                hook.RegisterHotKey(ModifierKeys.Control, Keys.PrintScreen);   //region
                hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.PrintScreen); //undo last
                hook.RegisterHotKey(ModifierKeys.Control, Keys.PageUp); //clipboard
                return true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show("Superscrot can't start because the hotkey is already registered."
                    + "\n\nIf Windows needs to restart to apply updates, please try rebooting."
                    + "\n\nError code: 0x" + ex.NativeErrorCode.ToString("X"),
                    "Superscrot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Releases resources used by the <see cref="Superscrot.Manager"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Captures a screenshot of the primary screen and uploads it to FTP.
        /// </summary>
        public void TakeAndUploadDesktopScreenshot()
        {
            Screenshot capture = Screenshot.FromDesktop();
            if (capture != null)
            {
                capture.Uploaded += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                        Clipboard.SetText(capture.PublicUrl);
                };
                UploadAsync(capture);
            }
        }

        /// <summary>
        /// Captures a screenshot of the active window and uploads it to FTP.
        /// </summary>
        public void TakeAndUploadWindowScreenshot()
        {
            Screenshot capture = Screenshot.FromActiveWindow();
            if (capture != null)
            {
                capture.Uploaded += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                        Clipboard.SetText(capture.PublicUrl);
                };
                UploadAsync(capture);
            }
        }

        /// <summary>
        /// Spawns the overlay to let the user draw a region, and captures it and uploads it to FTP.
        /// </summary>
        public void TakeAndUploadRegionScreenshot()
        {
            Screenshot capture = Screenshot.FromRegion();
            if (capture != null)
            {
                capture.Uploaded += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                        Clipboard.SetText(capture.PublicUrl);
                };
                UploadAsync(capture);
            }
        }

        /// <summary>
        /// Uploads images and files on the clipboard to FTP.
        /// </summary>
        public void UploadClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                Screenshot capture = Screenshot.FromClipboard();
                if (capture != null)
                {
                    capture.Uploaded += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                            Clipboard.SetText(capture.PublicUrl);
                    };
                    UploadAsync(capture);
                }
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringBuilder clipText = new StringBuilder();

                System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();

                if (files.Count == 1)
                {
                    Screenshot capture = Screenshot.FromFile(files[0]);
                    if (capture != null)
                    {
                        capture.Uploaded += (sender, e) =>
                        {
                            if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                                Clipboard.SetText(capture.PublicUrl);
                        };
                        UploadAsync(capture);
                    }
                }
                else
                {
                    var multiUploadThread = new Thread(() =>
                    {
                        foreach (string file in files)
                        {
                            try
                            {
                                if (!IsImageFile(file)) continue;

                                Screenshot capture = Screenshot.FromFile(file);
                                if (capture != null)
                                {
                                    var name = capture.GetFileName();
                                    var target = PathUtility.UriCombine(Program.Config.FtpServerPath, name);
                                    Upload(capture, target);
                                    if (!string.IsNullOrWhiteSpace(capture.PublicUrl))
                                        clipText.AppendLine(capture.PublicUrl);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                                System.Media.SystemSounds.Exclamation.Play();
                            }
                        }

                        if (clipText.Length > 0)
                            Clipboard.SetText(clipText.ToString().Trim());
                    });
                    multiUploadThread.SetApartmentState(ApartmentState.STA);
                    multiUploadThread.Start();
                }
            }
        }

        /// <summary>
        /// Uploads the screenshot in a new thread.
        /// </summary>
        /// <param name="screenshot">The screenshot to upload.</param>
        public void UploadAsync(Screenshot screenshot)
        {
            UploadAsync(screenshot, Program.Config.ShowPreviewDialog);
        }

        /// <summary>
        /// Uploads the screenshot in a new thread.
        /// </summary>
        /// <param name="screenshot">The screenshot to upload.</param>
        /// <param name="showPreview">Whether or not to show a preview before uploading.</param>
        public void UploadAsync(Screenshot screenshot, bool showPreview)
        {
            try
            {
                string filename = screenshot.GetFileName();

                if (showPreview)
                {
                    using (PreviewDialog preview = new PreviewDialog(screenshot))
                    {
                        if (preview.ShowDialog() == DialogResult.OK)
                            filename = preview.FileName;
                        else
                            return;
                    }
                }

                string target = PathUtility.UriCombine(Program.Config.FtpServerPath, filename);
                string url = PathUtility.UriCombine(Program.Config.HttpBaseUri, PathUtility.UrlEncode(filename));

                Thread uploadThread = new Thread(() =>
                {
                    Upload(screenshot, target);
                });
                uploadThread.SetApartmentState(ApartmentState.STA);
                uploadThread.Name = "Upload thread";
                uploadThread.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        /// <summary>
        /// Deletes the last uploaded file. Can be called multiple times consecutively.
        /// </summary>
        public void UndoUploadAsync()
        {
            try
            {
                if (History.Count == 0) return;

                Screenshot screenshot = History.Pop();
                Thread deleteThread = new Thread(() =>
                {
                    UndoUpload(screenshot);
                });
                deleteThread.Name = "Delete thread";
                deleteThread.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        /// <summary>
        /// Releases resources used by the <see cref="Superscrot.Manager"/> class.
        /// </summary>
        /// <param name="disposing">True to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                if (hook != null)
                {
                    hook.Dispose();
                    hook = null;
                }

                if (uploader != null)
                {
                    uploader.Dispose();
                    uploader = null;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="EnabledChanged"/> event.
        /// </summary>
        protected virtual void OnEnabledChanged()
        {
            var handler = EnabledChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns whether the specified file is an image file.
        /// </summary>
        /// <param name="file">The name of the file.</param>
        /// <returns>True if the specified file is a supported image file, otherwise false.</returns>
        private static bool IsImageFile(string file)
        {
            string ext = Path.GetExtension(file);
            string[] recognizedExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".gif" };
            foreach (string recognizedExtension in recognizedExtensions)
            {
                if (string.Compare(ext, recognizedExtension, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Uploads a screenshot to the specified file on the server.
        /// </summary>
        /// <param name="screenshot">The screenshot to upload.</param>
        /// <param name="target">The name of the file on the server that the screenshot will be uploaded to.</param>
        private void Upload(Screenshot screenshot, string target)
        {
            try
            {
                Uploader.Upload(screenshot, target);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                ReportUploadError(screenshot);
            }
            finally
            {
                screenshot.Dispose(); //TODO: don't dispose, rather flush to disk or remove local copy from disk
            }
        }

        private void HandleDuplicateFileFound(object sender, DuplicateFileEventArgs e)
        {
            Trace.WriteLine("Duplicate file found: " + e.FileName);

            using (var dialog = new Dialogs.DuplicateFileFoundDialog(e.Screenshot, e.FileName))
            {
                var result = dialog.ShowDialog();
                switch (result)
                {
                    case DialogResult.Ignore:
                        e.Action = DuplicateFileAction.Ignore;
                        break;
                    case DialogResult.Yes:
                        e.Action = DuplicateFileAction.Replace;
                        break;
                    case DialogResult.Abort:
                    default:
                        e.Action = DuplicateFileAction.Abort;
                        break;
                }
            }
        }

        /// <summary>
        /// Reports an uploading error to the user.
        /// </summary>
        /// <param name="screenshot">The screenshot that failed to upload.</param>
        private void ReportUploadError(Screenshot screenshot = null)
        {
            try
            {
                Program.Tray.ShowError("Screenshot was not successfully uploaded", string.Format("Check your connection to {0} and try again.", Program.Config.FtpHostname));
                System.Media.SystemSounds.Exclamation.Play();

                var fileName = PathUtility.RemoveInvalidFilenameChars(screenshot.GetFileName());
                var target = Path.Combine(Program.Config.FailedScreenshotsFolder, fileName);
                screenshot.SaveToFile(target);
                Trace.WriteLine("Failed screenshot saved to " + target);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        /// Gets an <see cref="Uploader"/> for the current configuration.
        /// </summary>
        private Uploader Uploader
        {
            get
            {
                if (uploader == null)
                {
                    uploader = Uploader.Create(Program.Config);
                    uploader.DuplicateFileFound += HandleDuplicateFileFound;
                    uploader.UploadSucceeded += (s) =>
                    {
                        History.Push(s);
                        System.Media.SystemSounds.Asterisk.Play();
                    };
                    uploader.UploadFailed += (s) =>
                    {
                        ReportUploadError(s);
                    };
                    uploader.DeleteSucceeded += (s) =>
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                    };
                    uploader.DeleteFailed += (s) =>
                    {
                        Trace.WriteLine("Screenshot could not be deleted from " 
                            + s.PublicUrl);
                        System.Media.SystemSounds.Exclamation.Play();
                        Program.Tray.ShowError("Screenshot could not be deleted", null);
                    };
                }

                return uploader;
            }
        }

        /// <summary>
        /// Deletes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The screenshot to delete.</param>
        private void UndoUpload(Screenshot screenshot)
        {
            try
            {
                Uploader.UndoUpload(screenshot);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                System.Media.SystemSounds.Exclamation.Play();
                Program.Tray.ShowError("Screenshot could not be deleted", null);
            }
        }

        /// <summary>
        /// Handles keyboard input.
        /// </summary>
        private void KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (!Enabled) return;

            try
            {
                if (e.Key == Keys.PrintScreen)
                {
                    switch (e.Modifier)
                    {
                        case ModifierKeys.None:
                            TakeAndUploadDesktopScreenshot();
                            break;
                        case ModifierKeys.Alt:
                            TakeAndUploadWindowScreenshot();
                            break;
                        case ModifierKeys.Control:
                            TakeAndUploadRegionScreenshot();
                            break;
                        case ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift:
                            UndoUploadAsync();
                            break;
                    }
                }
                else if (e.Key == Keys.PageUp && e.Modifier == ModifierKeys.Control)
                {
                    UploadClipboard();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                System.Media.SystemSounds.Exclamation.Play();
                Program.Tray.ShowError("Superscrot encountered a problem", "If this problem keeps happening, please report the problem at https://github.com/horsedrowner/Superscrot/issues \nDetails: " + ex.Message);
            }
        }
    }
}
