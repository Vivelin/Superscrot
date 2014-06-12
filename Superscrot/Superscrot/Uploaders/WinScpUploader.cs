#if WINSCP
using System;
using System.Collections.Generic;
using System.IO;
using WinSCP;

namespace Superscrot.Uploaders
{
    class WinScpUploader : IUploader
    {
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.Magenta, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.Magenta, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.Magenta, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.Magenta, format, arg); }

        public event UploadEventHandler UploadSucceeded;
        public event UploadEventHandler UploadFailed;
        public event UploadEventHandler DeleteSucceeded;
        public event UploadEventHandler DeleteFailed;

        public bool Upload(Screenshot screenshot, string target)
        {
            try
            {
                using (var session = GetSession())
                {
                    var targetDir = Path.GetDirectoryName(target).Replace("\\", "/");
                    var mkdirResult = session.ExecuteCommand("mkdir -p \"" + targetDir + "\"");
                    WriteLine(mkdirResult.Output);
                    WriteLine(mkdirResult.ErrorOutput);

                    var local = screenshot.SaveToFile(); // WinSCP doesn't support uploading streams
                    var transferResult = session.PutFiles(local, target);
                    transferResult.Check(); // Throws if upload failed

                    if (screenshot.Source != ScreenshotSource.File)
                    {
                        File.Delete(local);
                    }

                    screenshot.ServerPath = target;
                    if (UploadSucceeded != null)
                        UploadSucceeded(screenshot);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                if (UploadFailed != null)
                    UploadFailed(screenshot);
                return false;
            }
        }

        public bool UndoUpload(Screenshot screenshot)
        {
            try
            {
                using (var session = GetSession())
                {
                    var result = session.RemoveFiles(screenshot.ServerPath);
                    result.Check();

                    screenshot.ServerPath = null;
                    if (DeleteSucceeded != null)
                        DeleteSucceeded(screenshot);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                if (DeleteFailed != null)
                    DeleteFailed(screenshot);
                return false;
            }
        }

        private Session GetSession()
        {
            var sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = Program.Config.FtpHostname,
                UserName = Program.Config.FtpUsername,
                PortNumber = Program.Config.FtpPort,
                TimeoutInMilliseconds = Program.Config.FtpTimeout,
                Password = Program.Config.FtpPassword,
                SshHostKeyFingerprint = Program.Config.HostKeyFingerprint,
                SshPrivateKeyPath = Program.Config.PrivateKeyPath
            };

            var session = new Session();
            session.ExecutablePath = Program.Config.WinScpPath;
            session.OutputDataReceived += (sender, e) =>
            {
                WriteLine(e.Data);
            };
            session.Open(sessionOptions);
            return session;
        }
    }
}
#endif