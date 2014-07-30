using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Provides the functionality to upload and delete screenshot to and from an FTP server.
    /// </summary>
    class FtpUploader : IUploader
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
        /// Occurs when a duplicate file was found on the server before the screenshot was uploaded.
        /// </summary>
        public event EventHandler<DuplicateFileEventArgs> DuplicateFileFound;

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        public bool Upload(Screenshot screenshot, string target)
        {
            FTP.FtpClient ftp = new FTP.FtpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            ftp.Timeout = Program.Config.FtpTimeout;

            if (!ftp.AttemptConnection())
                throw new ConnectionFailedException(string.Format("Upload failed: can't connect to \"{0}\"", Program.Config.FtpHostname), Program.Config.FtpHostname);

            if (!ftp.DirectoryExists(Path.GetDirectoryName(target)))
                ftp.CreateDirectory(Path.GetDirectoryName(target));

            if (Program.Config.CheckForDuplicateFiles && !FindDuplicateFile(screenshot, ref target, ftp))
                return false;

            using (MemoryStream stream = new MemoryStream())
            {
                screenshot.SaveToStream(stream);
                if (ftp.Upload(stream, target))
                {
                    screenshot.ServerPath = target;
                    if (UploadSucceeded != null)
                        UploadSucceeded(screenshot);
                    return true;
                }
                else
                {
                    if (UploadFailed != null)
                        UploadFailed(screenshot);
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks if the session contains a duplicate file and returns 
        /// whether to continue the upload.
        /// </summary>
        /// <param name="screenshot">The screenshot that is being uploaded.</param>
        /// <param name="target">The target file name.</param>
        /// <param name="ftp">The session in which the upload is taking place.</param>
        /// <returns>False if the upload should be aborted.</returns>
        private bool FindDuplicateFile(Screenshot screenshot, ref string target, FTP.FtpClient ftp)
        {
            if (string.IsNullOrEmpty(screenshot.OriginalFileName)) return false;

            var handler = DuplicateFileFound;
            if (handler != null)
            {
                // lol
                Program.ConsoleWriteLine(ConsoleColor.White, "This shit is untested. If it " +
                    "does something unexpected or nothing at all, please create an issue at " +
                    "https://github.com/horsedrowner/Superscrot/issues");

                var directory = Path.GetDirectoryName(target).Replace('\\', '/');
                var listing = ftp.ListDirectory(directory);
                var name = Path.GetFileNameWithoutExtension(screenshot.OriginalFileName);
                var duplicate = listing.FirstOrDefault(x =>
                    x.Contains(name)
                );

                if (!string.IsNullOrEmpty(duplicate))
                {
                    var e = new DuplicateFileEventArgs(screenshot, Program.Config.FtpHostname, duplicate);

                    handler(this, e);
                    switch (e.Action)
                    {
                        case DuplicateFileAction.Replace:
                            if (duplicate.StartsWith("/"))
                                target = duplicate;
                            target = Common.UriCombine(directory, duplicate);
                            Program.ConsoleWriteLine(ConsoleColor.Magenta, "Changed target to {0}", target);
                            return true;
                        case DuplicateFileAction.Abort:
                            return false;
                        case DuplicateFileAction.Ignore:
                        default:
                            return true;
                    }
                }
            }

            return true;
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
            FTP.FtpClient ftp = new FTP.FtpClient(Program.Config.FtpHostname, Program.Config.FtpPort, Program.Config.FtpUsername, Program.Config.FtpPassword);
            ftp.Timeout = Program.Config.FtpTimeout;

            if (!ftp.AttemptConnection())
                throw new ConnectionFailedException(string.Format("Undo upload failed: can't connect to \"{0}\"", Program.Config.FtpHostname), Program.Config.FtpHostname);

            if (ftp.DeleteFile(screenshot.ServerPath))
            {
                if (DeleteSucceeded != null)
                    DeleteSucceeded(screenshot);
                return true;
            }
            else
            {
                if (DeleteFailed != null)
                    DeleteFailed(screenshot);
                return false;
            }
        }

    }
}
