using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Superscrot.Uploaders
{
    /// <summary>
    /// Represents the possible actions that can be taken when a duplicate file was found.
    /// </summary>
    public enum DuplicateFileAction
    {
        /// <summary>
        /// Indicates that the file should uploaded as-is, ignoring the existing file on the server.
        /// </summary>
        Ignore,

        /// <summary>
        /// Indicates that the file on the server should be replaced with the file being uploaded.
        /// </summary>
        Replace,

        /// <summary>
        /// Indicates that the upload should be aborted.
        /// </summary>
        Abort
    }

    /// <summary>
    /// Provides data for the <see cref="Uploader.DuplicateFileFound"/> event.
    /// </summary>
    public class DuplicateFileEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateFileEventArgs"/> class with the
        /// screenshot being uploaded, the hostname of the server that contains the duplicate file 
        /// and the name of the duplicate file on the server.
        /// </summary>
        /// <param name="screenshot">The <see cref="Screenshot"/> being uploaded.</param>
        /// <param name="hostname">The hostname of the server.</param>
        /// <param name="fileName">The name of the duplicate file on the server.</param>
        public DuplicateFileEventArgs(Screenshot screenshot, string hostname, string fileName)
        {
            Screenshot = screenshot;
            Hostname = hostname;
            FileName = fileName;
            Action = DuplicateFileAction.Ignore;
        }

        /// <summary>
        /// Gets the <see cref="Screenshot"/> that caused the event to trigger.
        /// </summary>
        public Screenshot Screenshot { get; private set; }

        /// <summary>
        /// Gets the hostname of the server on which the duplicate file exists.
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// Gets the name of the file on the server that is a duplicate of the screenshot being uploaded.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the action to be taken.
        /// </summary>
        public DuplicateFileAction Action { get; set; }
    }
}
