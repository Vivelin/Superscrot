using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from an FTP server.
    /// </summary>
    class FtpUploader : Uploader
    {
        private FTP.FtpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpUploader"/> class
        /// with the specified username and password.
        /// </summary>
        /// <param name="info">An object containing the connection info.</param>
        /// <param name="timeout">The time in milliseconds to wait for a 
        /// response from the server.</param>
        public FtpUploader(ConnectionInfo info, int timeout = 30000)
        {
            client = new FTP.FtpClient(info.Host, info.Port, info.UserName, 
                info.Password);
            client.Timeout = timeout;
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
            EnsureConnection();

            if (!client.DirectoryExists(Path.GetDirectoryName(target)))
                client.CreateDirectory(Path.GetDirectoryName(target));

            if (Program.Config.CheckForDuplicateFiles
                && !FindDuplicateFile(screenshot, ref target))
                return false;

            using (MemoryStream stream = new MemoryStream())
            {
                screenshot.Save(stream);
                if (client.Upload(stream, target))
                {
                    screenshot.ServerPath = target;
                    OnUploadSucceeded(screenshot);
                    return true;
                }
                else
                {
                    OnUploadFailed(screenshot);
                    return false;
                }
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
            EnsureConnection();

            if (client.DeleteFile(screenshot.ServerPath))
            {
                OnDeleteSucceeded(screenshot);
                return true;
            }
            else
            {
                OnDeleteFailed(screenshot);
                return false;
            }
        }

        private void EnsureConnection()
        {
            if (!client.AttemptConnection())
            {
                throw new ConnectionFailedException(
                    SR.ConnectionFailed.With(client.Hostname), client.Hostname);
            }
        }

        /// <summary>
        /// Checks if the session contains a duplicate file and returns 
        /// whether to continue the upload.
        /// </summary>
        /// <param name="screenshot">
        /// The screenshot that is being uploaded.
        /// </param>
        /// <param name="target">The target file name.</param>
        /// <returns>False if the upload should be aborted.</returns>
        private bool FindDuplicateFile(Screenshot screenshot, ref string target)
        {
            if (string.IsNullOrEmpty(screenshot.OriginalFileName)) return true;

            var directory = Path.GetDirectoryName(target).Replace('\\', '/');
            var listing = client.ListDirectory(directory);
            var name = Path.GetFileNameWithoutExtension(screenshot.OriginalFileName);
            var duplicate = listing.FirstOrDefault(x =>
                x.Contains(name)
            );

            if (!string.IsNullOrEmpty(duplicate))
            {
                var e = new DuplicateFileEventArgs(screenshot, Program.Config.FtpHostname, duplicate);

                OnDuplicateFileFound(e);
                switch (e.Action)
                {
                    case DuplicateFileAction.Replace:
                        if (duplicate.StartsWith("/"))
                            target = duplicate;
                        target = PathUtility.UriCombine(directory, duplicate);
                        Trace.WriteLine("Changed upload target to " + target);
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
    }
}
