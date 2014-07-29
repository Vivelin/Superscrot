using Superscrot.Uploaders;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Coordinates top-level functionality and provides common functions that interact between 
    /// classes. Console output is marked Cyan.
    /// </summary>
    public class Manager : IDisposable
    {
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.Cyan, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.Cyan, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.Cyan, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.Cyan, format, arg); }

        private KeyboardHook _hook = null;
        private History _history = null;

        /// <summary>
        /// Provides information about taken screenshots.
        /// </summary>
        public History History
        {
            get
            {
                if (_history == null)
                {
                    _history = new History();
                }
                return _history;
            }
        }

        /// <summary>
        /// Captures a screenshot of the primary screen and uploads it to FTP.
        /// </summary>
        public void TakeAndUploadDesktopScreenshot()
        {
            Screenshot capture = Screenshot.FromDesktop();
            if (capture != null)
            {
                string publicpath = UploadAsync(capture);
                if (!string.IsNullOrWhiteSpace(publicpath))
                    Clipboard.SetText(publicpath);
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
                string publicpath = UploadAsync(capture);
                if (!string.IsNullOrWhiteSpace(publicpath))
                    Clipboard.SetText(publicpath);
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
                string publicpath = UploadAsync(capture);
                if (!string.IsNullOrWhiteSpace(publicpath))
                    Clipboard.SetText(publicpath);
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
                    string publicpath = UploadAsync(capture);
                    if (!string.IsNullOrWhiteSpace(publicpath))
                        Clipboard.SetText(publicpath);
                }
            }
            else if (Clipboard.ContainsFileDropList())
            {
                WriteLine("Clipboard contains files: ");
                StringBuilder clipText = new StringBuilder();

                System.Collections.Specialized.StringCollection files = Clipboard.GetFileDropList();
                bool showPreview = (files.Count > 1 ? false : Program.Config.ShowPreviewDialog);
                if (!showPreview) WriteLine("Skipping preview dialog for multiple files");

                foreach (string file in files)
                {
                    try
                    {
                        if (!IsImageFile(file))
                        {
                            WriteLine("{0} is not recognized by superscrot", file);
                            continue;
                        }

                        Screenshot capture = Screenshot.FromFile(file);
                        if (capture != null)
                        {
                            string publicpath = UploadAsync(capture, showPreview);
                            if (!string.IsNullOrWhiteSpace(publicpath))
                                clipText.AppendLine(publicpath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.ConsoleException(ex);
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                }

                if (clipText.Length > 0)
                    Clipboard.SetText(clipText.ToString().Trim());
            }
            else
            {
                WriteLine("Clipboard is empty or clipboard contains data that is not supported by superscrot");
            }
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
        /// Uploads the screenshot in a new thread.
        /// </summary>
        /// <param name="screenshot">The screenshot to upload.</param>
        /// <returns>The public URL to the uploaded screenshot.</returns>
        public string UploadAsync(Screenshot screenshot)
        {
            return UploadAsync(screenshot, Program.Config.ShowPreviewDialog);
        }

        /// <summary>
        /// Uploads the screenshot in a new thread.
        /// </summary>
        /// <param name="screenshot">The screenshot to upload.</param>
        /// <param name="showPreview">Whether or not to show a preview before uploading.</param>
        /// <returns>The public URL to the uploaded screenshot.</returns>
        public string UploadAsync(Screenshot screenshot, bool showPreview)
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
                        {
                            WriteLine("Cancelled");
                            return null;
                        }
                    }
                }

                string target = Common.UriCombine(Program.Config.FtpServerPath, filename);
                string url = Common.UriCombine(Program.Config.HttpBaseUri, Common.UrlEncode(filename));

                Thread uploadThread = new Thread(() =>
                {
                    Upload(screenshot, target);
                });
                uploadThread.Name = "Upload thread";
                uploadThread.Start();

                WriteLine("[0x{0:X}] Uploading to {1}...", uploadThread.ManagedThreadId, url);
                return url;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                return null;
            }
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
                var up = GetUploader();
                if (up.Upload(screenshot, target))
                {
                    History.Push(screenshot);
                    System.Media.SystemSounds.Asterisk.Play();
                    WriteLine("[0x{0:X}] Upload succeeded", Thread.CurrentThread.ManagedThreadId);
                }
                else
                {
                    Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Upload failed!", Thread.CurrentThread.ManagedThreadId);
                    ReportUploadError(screenshot);
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                ReportUploadError(screenshot);
            }
            finally
            {
                screenshot.Dispose(); //TODO: don't dispose, rather flush to disk or remove local copy from disk
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

                var fileName = Common.RemoveInvalidFilenameChars(screenshot.GetFileName());
                var target = Path.Combine(Program.Config.FailedScreenshotsFolder, fileName);
                screenshot.SaveToFile(target);
                WriteLine("Failed screenshot saved to {0}", target);
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }
        }

        /// <summary>
        /// Deletes the last uploaded file. Can be called multiple times consecutively.
        /// </summary>
        public void UndoUploadAsync()
        {
            try
            {
                if (History.Count == 0)
                {
                    WriteLine("Nothing to undo");
                    return;
                }

                Screenshot screenshot = History.Pop();

                Thread deleteThread = new Thread(() =>
                {
                    UndoUpload(screenshot);
                });
                deleteThread.Name = "Delete thread";
                deleteThread.Start();

                WriteLine("[0x{1:X}] Removing {0} from server...", screenshot.ServerPath, deleteThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        /// <summary>
        /// Returns an uploader for the current configuration.
        /// </summary>
        /// <returns>Returns a newly created <see cref="Superscrot.Uploaders.IUploader"/> instance.</returns>
        private static IUploader GetUploader()
        {
            if (Program.Config.UseSSH)
            {
#if WINSCP
                //if (File.Exists(Program.Config.WinScpPath))
                //    return new WinScpUploader();
#endif
                return new SftpUploader();
            }

            return new FtpUploader();
        }

        /// <summary>
        /// Deletes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The screenshot to delete.</param>
        private static void UndoUpload(Screenshot screenshot)
        {
            try
            {
                var up = GetUploader();
                if (up.UndoUpload(screenshot))
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    WriteLine("[0x{0:X}] File deleted", Thread.CurrentThread.ManagedThreadId);
                }
                else
                {
                    System.Media.SystemSounds.Exclamation.Play();
                    Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Deletion failed!", Thread.CurrentThread.ManagedThreadId);
                    Program.Tray.ShowError("Screenshot could not be deleted", null);
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                Program.Tray.ShowError("Screenshot could not be deleted", null);
            }
        }

        /// <summary>
        /// Initializes the global keyboard hook.
        /// </summary>
        public bool InitializeKeyboardHook()
        {
            try
            {
                _hook = new KeyboardHook();
                _hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyPressed);
                _hook.RegisterHotKey(ModifierKeys.None, Keys.PrintScreen);      //desktop screenshot
                _hook.RegisterHotKey(ModifierKeys.Alt, Keys.PrintScreen);       //active window
                _hook.RegisterHotKey(ModifierKeys.Control, Keys.PrintScreen);   //region
                _hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.PrintScreen); //undo last
                _hook.RegisterHotKey(ModifierKeys.Control, Keys.PageUp); //clipboard
                return true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Program.ConsoleFatal(ex);
                MessageBox.Show("Superscrot can't start because the hotkey is already registered."
                    + "\n\nIf Windows needs to restart to apply updates, please try rebooting."
                    + "\n\nError code: 0x" + ex.NativeErrorCode.ToString("X"),
                    "Superscrot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Releases resources used by the <c>Superscrot.Manager</c> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                if (_hook != null)
                {
                    _hook.Dispose();
                    _hook = null;
                }
            }
        }

        /// <summary>
        /// Handles keyboard input.
        /// </summary>
        private void KeyPressed(object sender, KeyPressedEventArgs e)
        {
            Write("Pressed ");
            if (e.Modifier != ModifierKeys.None) Write(e.Modifier.ToString() + " + ");
            WriteLine(e.Key.ToString());

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
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                Program.Tray.ShowError("Superscrot encountered a problem", "If this problem keeps happening, please report the problem at https://github.com/horsedrowner/Superscrot/issues \nDetails: " + ex.Message);
            }
        }
    }
}
