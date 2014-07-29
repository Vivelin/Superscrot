﻿#if WINSCP
using System;
using System.Collections.Generic;
using System.IO;
using WinSCP;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from an SFTP server using WinSCP.
    /// </summary>
    class WinScpUploader : IUploader
    {
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.Magenta, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.Magenta, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.Magenta, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.Magenta, format, arg); }

        /// <summary>
        /// Occurs when an upload has succeeded.
        /// </summary>
        public event UploadEventHandler UploadSucceeded;

        /// <summary>
        /// Occurs when an upload has failed.
        /// </summary>
        public event UploadEventHandler UploadFailed;

        /// <summary>
        /// Occurs when a file was deleted succesfully.
        /// </summary>
        public event UploadEventHandler DeleteSucceeded;

        /// <summary>
        /// Occurs when a file could not be deleted.
        /// </summary>
        public event UploadEventHandler DeleteFailed;

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        public bool Upload(Screenshot screenshot, string target)
        {
            try
            {
                using (var session = GetSession())
                {
                    var local = screenshot.SaveToFile(); // WinSCP doesn't support uploading streams
                    var transferResult = session.PutFiles(local, target);

                    /* If the upload failed, it's possible the directory doesn't exist. However, 
                     * ExecuteCommand seems to always open a new session internally, so only do
                     * this when it fails 
                     */
                    if (!transferResult.IsSuccess)
                    {
                        var targetDir = Path.GetDirectoryName(target).Replace("\\", "/");
                        var mkdirResult = session.ExecuteCommand("mkdir -p \"" + targetDir + "\"");
                        WriteLine(mkdirResult.Output);
                        WriteLine(mkdirResult.ErrorOutput);

                        // Retry the upload
                        transferResult = session.PutFiles(local, target);
                    }

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

        /// <summary>
        /// Removes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to remove from the server.</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>        
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        public bool UndoUpload(Screenshot screenshot)
        {
            if (screenshot.ServerPath == null) throw new InvalidOperationException("Can't undo an upload that never happened");

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