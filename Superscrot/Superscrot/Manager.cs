using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Renci.SshNet;
using System.Text;

namespace Superscrot
{
    /// <summary>
    /// Coordinates top-level functionality and provides common functions that interact between 
    /// classes. Console output is marked Cyan.
    /// </summary>
    public class Manager
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
                string publicpath = StartUploadAsync(capture);
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
                string publicpath = StartUploadAsync(capture);
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
                string publicpath = StartUploadAsync(capture);
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
                    string publicpath = StartUploadAsync(capture);
                    Clipboard.SetText(publicpath);
                }
            }
            else if (Clipboard.ContainsFileDropList())
            {
                WriteLine("Clipboard contains files: ");
                StringBuilder clipText = new StringBuilder();
                foreach (string file in Clipboard.GetFileDropList())
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
                            string publicpath = StartUploadAsync(capture);
                            clipText.AppendLine(publicpath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.ConsoleException(ex);
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                }
                Clipboard.SetText(clipText.ToString().Trim());
            }
            else
            {
                WriteLine("Clipboard is empty or clipboard contains data that is not supported by superscrot");
            }
        }

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

        public string StartUploadAsync(Screenshot screenshot)
        {
            try
            {
                string filename = screenshot.GetFileName();
                screenshot.ServerPath = Common.UriCombine(Program.Config.FtpServerPath, filename);
                screenshot.PublicPath = Common.UriCombine(Program.Config.HttpBaseUri, Common.UrlEncode(filename));

                Thread uploadThread = new Thread(() =>
                {
                    try
                    {
                        if (Program.Config.UseSSH)
                            UploadSftp(screenshot);
                        else
                            UploadFtp(screenshot);
                    }
                    catch (Exception ex)
                    {
                        Program.ConsoleException(ex);
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                    finally
                    {
                        screenshot.Dispose(); //TODO: don't dispose, rather flush to disk or remove local copy from disk
                    }
                });
                uploadThread.Name = "Upload thread";
                uploadThread.Start();

                WriteLine("[0x{0:X}] Uploading to {1}...", uploadThread.ManagedThreadId, screenshot.PublicPath);
                return screenshot.PublicPath;

            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                return null;
            }
        }

        /// <summary>
        /// Uploads a <c>System.IO.Stream</c> to a file on the configured SFTP server.
        /// </summary>
        private void UploadSftp(Screenshot screenshot)
        {
            SftpClient c = new SftpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            c.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, Program.Config.FtpTimeout);
            c.Connect();
            if (!c.IsConnected)
            {
                Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Upload failed: can't connect to server. Check your settings and try again later.", Thread.CurrentThread.ManagedThreadId);
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            string folder = Path.GetDirectoryName(screenshot.ServerPath).Replace('\\', '/');
            SftpCreateDirectoryRecursive(ref c, folder);

            using (MemoryStream stream = new MemoryStream())
            {
                screenshot.SaveToStream(stream);
                c.UploadFile(stream, screenshot.ServerPath);
            }

            WriteLine("[0x{0:X}] Upload completed!", Thread.CurrentThread.ManagedThreadId);
            History.Push(screenshot);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private static void SftpCreateDirectoryRecursive(ref SftpClient c, string path)
        {
            if (!c.Exists(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                SftpCreateDirectoryRecursive(ref c, parent);
                WriteLine("[0x{0:X}] Creating directory {1}", Thread.CurrentThread.ManagedThreadId, path);
                c.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Uploads a <c>System.IO.Stream</c> to a file on the configured FTP server.
        /// </summary>
        private void UploadFtp(Screenshot screenshot)
        {
            FTP.FtpClient ftp = new FTP.FtpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            ftp.Timeout = Program.Config.FtpTimeout;

            if (!ftp.AttemptConnection())
            {
                Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Upload failed: can't connect to server. Check your settings and try again later.", Thread.CurrentThread.ManagedThreadId);
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            if (!ftp.DirectoryExists(Path.GetDirectoryName(screenshot.ServerPath)))
                ftp.CreateDirectory(Path.GetDirectoryName(screenshot.ServerPath));

            using (MemoryStream stream = new MemoryStream())
            {
                screenshot.SaveToStream(stream);
                if (ftp.Upload(stream, screenshot.ServerPath))
                {
                    WriteLine("[0x{0:X}] Upload completed!", Thread.CurrentThread.ManagedThreadId);
                    History.Push(screenshot);
                    System.Media.SystemSounds.Asterisk.Play();
                }
                else
                {
                    Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Upload failed!", Thread.CurrentThread.ManagedThreadId);
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
        }

        /// <summary>
        /// Deletes the last uploaded file. Can be called multiple times consecutively.
        /// </summary>
        public void UndoUpload()
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
                    try
                    {
                        if (Program.Config.UseSSH)
                            UndoUploadSftp(screenshot.ServerPath);
                        else
                            UndoUploadFtp(screenshot.ServerPath);
                    }
                    catch (Exception ex)
                    {
                        Program.ConsoleException(ex);
                        System.Media.SystemSounds.Exclamation.Play();
                    }
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
        /// Deletes the specified file from the configured SFTP server.
        /// </summary>
        /// <param name="serverpath">The path to the file on the server to be deleted.</param>
        private static void UndoUploadSftp(string serverpath)
        {
            SftpClient c = new SftpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            c.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, Program.Config.FtpTimeout);
            c.Connect();
            if (!c.IsConnected)
            {
                Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Delete failed: can't connect to server.", Thread.CurrentThread.ManagedThreadId);
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            c.DeleteFile(serverpath);
            WriteLine("[0x{0:X}] File deleted", Thread.CurrentThread.ManagedThreadId);
            System.Media.SystemSounds.Asterisk.Play();
        }

        /// <summary>
        /// Deletes the specified file from the configured FTP server.
        /// </summary>
        /// <param name="serverpath">The path to the file on the server to be deleted.</param>
        private static void UndoUploadFtp(string serverpath)
        {
            FTP.FtpClient ftp = new FTP.FtpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            ftp.Timeout = Program.Config.FtpTimeout;

            if (!ftp.AttemptConnection())
            {
                Program.ConsoleWriteLine(ConsoleColor.Yellow, "[0x{0:X}] Delete failed: can't connect to server.", Thread.CurrentThread.ManagedThreadId);
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            if (ftp.DeleteFile(serverpath))
            {
                WriteLine("[0x{0:X}] File deleted", Thread.CurrentThread.ManagedThreadId);
            }
            else
            {
                WriteLine("[0x{0:X} Delete failed!", Thread.CurrentThread.ManagedThreadId);
                System.Media.SystemSounds.Exclamation.Play();
            }
        }

        /// <summary>
        /// Initializes the global keyboard hook.
        /// </summary>
        public void InitializeKeyboardHook()
        {
            _hook = new KeyboardHook();
            _hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyPressed);
            _hook.RegisterHotKey(ModifierKeys.None, Keys.PrintScreen);      //desktop screenshot
            _hook.RegisterHotKey(ModifierKeys.Alt, Keys.PrintScreen);       //active window
            _hook.RegisterHotKey(ModifierKeys.Control, Keys.PrintScreen);   //region
            _hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, Keys.PrintScreen); //undo last
            _hook.RegisterHotKey(ModifierKeys.Control, Keys.PageUp); //clipboard
        }

        /// <summary>
        /// Releases resources used by the <c>Superscrot.Manager</c> class.
        /// </summary>
        public void Dispose()
        {
            if (_hook != null)
            {
                _hook.Dispose();
                _hook = null;
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
                        UndoUpload();
                        break;
                }
            }
            else if (e.Key == Keys.PageUp && e.Modifier == ModifierKeys.Control)
            {
                UploadClipboard();
            }
        }
    }
}
