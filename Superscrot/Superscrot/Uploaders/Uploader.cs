using System;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Represents the method that will handle events fired by the <see cref="Uploader"/> class.
    /// </summary>
    /// <param name="s">The screenshot for the event.</param>
    public delegate void UploadEventHandler(Screenshot s);   

    /// <summary>
    /// Represents an abstract base class for uploading and deleting screenshots.
    /// </summary>
    abstract class Uploader : IDisposable
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
        /// Returns a new instance of the <see cref="Uploader"/> class for the
        /// specified configuration.
        /// </summary>
        /// <param name="config">A <see cref="Configuration"/> object that 
        /// contains the connection info and settings.</param>
        /// <returns>A new object that derives from <see cref="Uploader"/>.
        /// </returns>
        public static Uploader Create(Configuration config)
        {
            var info = new ConnectionInfo(config);

            if (config.UseSSH)
            {
                return new SftpUploader(info, config.FtpTimeout);
            }
            else
            {
                return new FtpUploader(info, config.FtpTimeout);
            }
        }

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        public abstract bool Upload(Screenshot screenshot, string target);

        /// <summary>
        /// Removes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to remove from the server.</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        public abstract bool UndoUpload(Screenshot screenshot);

        /// <summary>
        /// Raises the <see cref="UploadSucceeded"/> event.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> that was 
        /// uploaded.</param>
        protected virtual void OnUploadSucceeded(Screenshot screenshot)
        {
            var uploadSucceeded = UploadSucceeded;
            if (uploadSucceeded != null)
            {
                uploadSucceeded(screenshot);
            }
        }

        /// <summary>
        /// Raises the <see cref="UploadFailed"/> event.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> that failed 
        /// to upload.</param>
        protected virtual void OnUploadFailed(Screenshot screenshot)
        {
            var uploadFailed = UploadFailed;
            if (uploadFailed != null)
            {
                uploadFailed(screenshot);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeleteSucceeded"/> event.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> that was 
        /// deleted.</param>
        protected virtual void OnDeleteSucceeded(Screenshot screenshot)
        {
            var deleteSucceeded = DeleteSucceeded;
            if (deleteSucceeded != null)
            {
                deleteSucceeded(screenshot);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeleteFailed"/> event.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> that failed 
        /// to delete.</param>
        protected virtual void OnDeleteFailed(Screenshot screenshot)
        {
            var deleteFailed = DeleteFailed;
            if (deleteFailed != null)
            {
                deleteFailed(screenshot);
            }
        }

        /// <summary>
        /// Raises the <see cref="DuplicateFileFound"/> event.
        /// </summary>
        /// <param name="e">A <see cref="DuplicateFileEventArgs"/> object that
        /// contains the event data.</param>
        protected virtual void OnDuplicateFileFound(DuplicateFileEventArgs e)
        {
            var duplicateFileFound = DuplicateFileFound;
            if (duplicateFileFound != null)
            {
                duplicateFileFound(this, e);
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
        protected virtual void Dispose(bool disposing) { }
    }
}
