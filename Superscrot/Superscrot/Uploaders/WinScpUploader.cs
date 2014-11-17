#if WINSCP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinSCP;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from an SFTP server using WinSCP.
    /// </summary>
    class WinScpUploader : Uploader, IDisposable
    {
        private WinSCP.Session session;

        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.Magenta, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.Magenta, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.Magenta, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.Magenta, format, arg); }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinScpUploader"/> 
        /// class.
        /// </summary>
        /// <param name="hostname">The hostname or IP address of the server.</param>
        /// <param name="port">The port number of the server.</param>
        /// <param name="username">The name of the user on the server.</param>
        /// <param name="fingerprint">The fingerprint of the server's public 
        /// key.</param>
        /// <param name="privateKeyPath">The path to the private key file to
        /// authenticate with.</param>
        /// <param name="timeout">The time in milliseconds to wait for a 
        /// response from the server.</param>
        public WinScpUploader(string hostname, int port, string username,
            string fingerprint, string privateKeyPath, int timeout = 30000)
        {
            var opt = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = hostname,
                PortNumber = port,
                UserName = username,
                SshHostKeyFingerprint = fingerprint,
                SshPrivateKeyPath = privateKeyPath,
                TimeoutInMilliseconds = timeout
            };

            session = new Session
            {
                DisableVersionCheck = true,
                ExecutablePath = Program.Config.WinScpPath
            };
            session.OutputDataReceived += (sender, e) => { WriteLine(e.Data); };

            session.Open(opt);
            if (!session.Opened)
            {
                throw new ConnectionFailedException(
                    string.Format("Upload failed: can't connect to \"{0}\"",
                        Program.Config.FtpHostname),
                    Program.Config.FtpHostname);
            }
        }

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        public override bool Upload(Screenshot screenshot, string target)
        {
            try
            {
                var local = screenshot.SaveToFile(); // WinSCP doesn't support uploading streams

                if (Program.Config.CheckForDuplicateFiles 
                    && !FindDuplicateFile(screenshot, ref target, session))
                    return false;

                var transferResult = session.PutFiles(local, target);
                if (!transferResult.IsSuccess)
                {
                    EnsureDirectoryExists(target, session);
                    transferResult = session.PutFiles(local, target);
                }
                transferResult.Check(); // Throws if upload failed

                if (screenshot.Source != ScreenshotSource.File)
                {
                    File.Delete(local);
                }

                screenshot.ServerPath = target;
                OnUploadSucceeded(screenshot);
                return true;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                OnUploadFailed(screenshot);
                return false;
            }
        }

        /// <summary>
        /// Removes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to remove from the server.</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>        
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        public override bool UndoUpload(Screenshot screenshot)
        {
            if (screenshot.ServerPath == null) throw new InvalidOperationException("Can't undo an upload that never happened");

            try
            {
                var result = session.RemoveFiles(screenshot.ServerPath);
                result.Check();

                screenshot.ServerPath = null;
                OnDeleteSucceeded(screenshot);
                return true;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                OnDeleteFailed(screenshot);
                return false;
            }
        }

        /// <summary>
        /// Cleans up resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up resources used by this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release managed resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (session != null)
                {
                    session.Dispose();
                    session = null;
                }
            }
        }

        /// <summary>
        /// Checks if the session contains a duplicate file and returns 
        /// whether to continue the upload.
        /// </summary>
        /// <param name="screenshot">The screenshot that is being uploaded.</param>
        /// <param name="target">The target file name.</param>
        /// <param name="session">The session in which the upload is taking place.</param>
        /// <returns>False if the upload should be aborted.</returns>
        private bool FindDuplicateFile(Screenshot screenshot, ref string target, Session session)
        {
            if (string.IsNullOrEmpty(screenshot.OriginalFileName)) return true;

            var directory = Path.GetDirectoryName(target).Replace('\\', '/');
            var listing = session.ListDirectory(directory);
            var name = Path.GetFileNameWithoutExtension(screenshot.OriginalFileName);
            var duplicate = listing.Files.FirstOrDefault(x =>
                x.Name.Contains(name)
            );

            if (duplicate != null)
            {
                var e = new DuplicateFileEventArgs(screenshot, Program.Config.FtpHostname, duplicate.Name);

                OnDuplicateFileFound(e);
                switch (e.Action)
                {
                    case DuplicateFileAction.Replace:
                        target = Common.UriCombine(directory, duplicate.Name);
                        WriteLine("Changed target to {0}", target);
                        return true;
                    case DuplicateFileAction.Abort:
                        return false;
                    case DuplicateFileAction.Ignore:
                    default:
                        return true;
                }
            }

            return true;
        }

        private static void EnsureDirectoryExists(string target, Session session)
        {
            var targetDir = Path.GetDirectoryName(target).Replace("\\", "/");
            var mkdirResult = session.ExecuteCommand("mkdir -p \"" + targetDir + "\"");
            WriteLine(mkdirResult.Output);
            WriteLine(mkdirResult.ErrorOutput);
        }
    }
}
#endif