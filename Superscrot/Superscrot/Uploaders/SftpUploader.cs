using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from an SFTP server.
    /// </summary>
    class SftpUploader : IUploader
    {
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
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        public bool Upload(Screenshot screenshot, string target)
        {
            SftpClient c = new SftpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            c.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, Program.Config.FtpTimeout);
            c.Connect();
            if (!c.IsConnected)
                throw new ConnectionFailedException(string.Format("Upload failed: can't connect to \"{0}\"", Program.Config.FtpHostname), Program.Config.FtpHostname);

            string folder = Path.GetDirectoryName(target).Replace('\\', '/');
            SftpCreateDirectoryRecursive(ref c, folder);

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    screenshot.SaveToStream(stream);
                    c.UploadFile(stream, target);
                }

                screenshot.ServerPath = target;
                if (UploadSucceeded != null)
                    UploadSucceeded(screenshot);
                return true;
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
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        public bool UndoUpload(Screenshot screenshot)
        {
            if (screenshot.ServerPath == null) throw new InvalidOperationException("Can't undo an upload that never happened");

            SftpClient c = new SftpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            c.ConnectionInfo.Timeout = new TimeSpan(0, 0, 0, 0, Program.Config.FtpTimeout);
            c.Connect();
            if (!c.IsConnected)
                throw new ConnectionFailedException(string.Format("Undo upload failed: can't connect to \"{0}\"", Program.Config.FtpHostname), Program.Config.FtpHostname);

            try
            {
                c.DeleteFile(screenshot.ServerPath);
                if (DeleteSucceeded != null)
                    DeleteSucceeded(screenshot);
                return true;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                if (DeleteFailed != null)
                    DeleteFailed(screenshot);
                return false;
            }
        }

        private static void SftpCreateDirectoryRecursive(ref SftpClient c, string path)
        {
            if (!c.Exists(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                SftpCreateDirectoryRecursive(ref c, parent);
                Program.ConsoleWriteLine(ConsoleColor.Magenta, "Creating directory {0}", path);
                c.CreateDirectory(path);
            }
        }

    }
}
