using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from 
    /// an SFTP server.
    /// </summary>
    class SftpUploader : Uploader
    {
        private SftpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpUploader"/> class
        /// with the specified <see cref="SftpClient"/>.
        /// </summary>
        /// <param name="info">An object containing the connection info.</param>
        /// <param name="timeout">The time in milliseconds to wait for a 
        /// response from the server.</param>
        public SftpUploader(ConnectionInfo info, int timeout = 30000)
        {
            try
            {
                var keyFile = new PrivateKeyFile(info.PrivateKeyPath);
                client = new SftpClient(info.Host, info.Port, info.UserName,
                    keyFile);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            if (client == null)
            {
                client = new SftpClient(info.Host, info.Port, info.UserName,
                    info.Password);
            }
            client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(timeout);
        }

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        public override bool Upload(Screenshot screenshot, string target)
        {
            if (screenshot == null)
                throw new ArgumentNullException("screenshot");

            EnsureConnection();

            string folder = Path.GetDirectoryName(target).Replace('\\', '/');
            SftpCreateDirectoryRecursive(folder);

            try
            {
                if (Program.Config.CheckForDuplicateFiles 
                    && !FindDuplicateFile(screenshot, ref target))
                    return false;

                using (MemoryStream stream = new MemoryStream())
                {
                    screenshot.SaveToStream(stream);
                    client.UploadFile(stream, target);
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
            finally
            {
                if (client != null)
                    client.Disconnect();
            }
        }

        /// <summary>
        /// Removes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to remove from the server.</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        public override bool UndoUpload(Screenshot screenshot)
        {
            if (screenshot == null)
                throw new ArgumentNullException("screenshot");
            if (screenshot.ServerPath == null) 
                throw new InvalidOperationException("Can't undo an upload that never happened");

            EnsureConnection();

            try
            {
                client.DeleteFile(screenshot.ServerPath);
                OnDeleteSucceeded(screenshot);
                return true;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                OnDeleteFailed(screenshot);
                return false;
            }
            finally
            {
                if (client != null)
                    client.Disconnect();
            }
        }

        /// <summary>
        /// Cleans up resources used by this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }

        private void EnsureConnection()
        {
            if (client != null)
            {
                client.Connect();
                if (!client.IsConnected)
                {
                    throw new ConnectionFailedException(
                        string.Format("Upload failed: can't connect to \"{0}\"",
                            client.ConnectionInfo.Host),
                        client.ConnectionInfo.Host);
                }
            }
        }

        /// <summary>
        /// Checks if the session contains a duplicate file and returns 
        /// whether to continue the upload.
        /// </summary>
        /// <param name="screenshot">The screenshot that is being uploaded.</param>
        /// <param name="target">The target file name.</param>
        /// <returns>False if the upload should be aborted.</returns>
        private bool FindDuplicateFile(Screenshot screenshot, ref string target)
        {
            if (string.IsNullOrEmpty(screenshot.OriginalFileName)) return true;

            var directory = Path.GetDirectoryName(target).Replace('\\', '/');
            var listing = client.ListDirectory(directory);
            var name = Path.GetFileNameWithoutExtension(screenshot.OriginalFileName);
            var duplicate = listing.FirstOrDefault(x =>
                x.Name.Contains(name)
            );

            if (duplicate != null)
            {
                var e = new DuplicateFileEventArgs(screenshot, 
                    client.ConnectionInfo.Host, duplicate.Name);

                OnDuplicateFileFound(e);
                switch (e.Action)
                {
                    case DuplicateFileAction.Replace:
                        target = duplicate.FullName;                            
                        Program.ConsoleWriteLine(ConsoleColor.Magenta, "Changed target to {0}", target);
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

        private void SftpCreateDirectoryRecursive(string path)
        {
            if (!client.Exists(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                SftpCreateDirectoryRecursive(parent);
                Program.ConsoleWriteLine(ConsoleColor.Magenta, 
                    "Creating directory {0}", path);
                client.CreateDirectory(path);
            }
        }
    }
}