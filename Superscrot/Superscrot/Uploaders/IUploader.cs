using System;
using System.Collections.Generic;
using System.Text;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Represents the method that will handle events fired by the <see cref="IUploader"/> class.
    /// </summary>
    /// <param name="s">The screenshot for the event.</param>
    public delegate void UploadEventHandler(Screenshot s);   

    /// <summary>
    /// Provides the functionality for upload and deleting screenshots.
    /// </summary>
    interface IUploader
    {
        /// <summary>
        /// Occurs when an upload has succeeded.
        /// </summary>
        event UploadEventHandler UploadSucceeded;

        /// <summary>
        /// Occurs when an upload has failed.
        /// </summary>
        event UploadEventHandler UploadFailed;

        /// <summary>
        /// Occurs when a file was deleted succesfully.
        /// </summary>
        event UploadEventHandler DeleteSucceeded;

        /// <summary>
        /// Occurs when a file could not be deleted.
        /// </summary>
        event UploadEventHandler DeleteFailed;

        /// <summary>
        /// Occurs when a duplicate file was found on the server before the screenshot was uploaded.
        /// </summary>
        event EventHandler<DuplicateFileEventArgs> DuplicateFileFound;

        /// <summary>
        /// Uploads a screenshot to the target location on the currently configured server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to upload.</param>
        /// <param name="target">The path on the server to upload to.</param>
        /// <returns>True if the upload succeeded, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        bool Upload(Screenshot screenshot, string target);

        /// <summary>
        /// Removes a screenshot from the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Superscrot.Screenshot"/> to remove from the server.</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>
        /// <exception cref="Superscrot.ConnectionFailedException">Connectioned to the server failed</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="screenshot"/> has not been uploaded (ServerPath property was not set)</exception>
        bool UndoUpload(Screenshot screenshot);
    }
}
