using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Specifies the source of a screenshot.
    /// </summary>
    public enum ScreenshotSource
    {
        /// <summary>
        /// The screenshot was taken from either the user's entire desktop, or one of his screens.
        /// </summary>
        Desktop,

        /// <summary>
        /// The screenshot originates from an image from the user's clipboard.
        /// </summary>
        Clipboard,

        /// <summary>
        /// The screenshot was taken from a user-selected region on the screen.
        /// </summary>
        RegionCapture,

        /// <summary>
        /// The screenshot was taken from the active window.
        /// </summary>
        WindowCapture,

        /// <summary>
        /// The screenshot originates from an image file.
        /// </summary>
        File
    }

    /// <summary>
    /// Represents a single taken screenshot.
    /// </summary>
    public class Screenshot : IDisposable
    {
        private const PixelFormat DefaultPixelFormat = PixelFormat.Format24bppRgb;

        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.DarkGreen, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.DarkGreen, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, format, arg); }

        private Bitmap _bitmap;

        /// <summary>
        /// Gets or sets the source of the screenshot.
        /// </summary>
        public ScreenshotSource Source { get; set; }

        /// <summary>
        /// Gets or sets a bitmap image of the screenshot.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        /// <summary>
        /// Gets or sets the path on the server, or null if the screenshot hasn't been uploaded yet.
        /// </summary>
        public string ServerPath { get; set; }

        /// <summary>
        /// Gets or sets the public URL to the file on the server, or null if the screenshot hasn't been uploaded yet.
        /// </summary>
        public string PublicPath { get; set; }

        /// <summary>
        /// Gets or sets the title of the window the screenshot was taken of, or null for non-window captures.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Gets or sets the original filename that the screenshot originates from, or null for non file-based captures.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Superscrot.Screenshot"/> class.
        /// </summary>
        public Screenshot()
        {

        }

        /// <summary>
        /// Releases all resources used by the <see cref="Superscrot.Screenshot"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Superscrot.Screenshot"/> class.
        /// </summary>
        /// <param name="disposing">True to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
            }
        }

        /// <summary>
        /// Retrieves an image with the contents of the primary screen.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> with the primary screen capture.</returns>
        public static Screenshot FromPrimaryScreen()
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.Desktop;

                int width = Screen.PrimaryScreen.Bounds.Width;
                int height = Screen.PrimaryScreen.Bounds.Height;
                Write("Taking shot in 5.. 4.. ");

                screenshot.Bitmap = new Bitmap(width, height, DefaultPixelFormat);
                using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                }
                WriteLine("just kidding, already done.", width, height);
                return screenshot;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }

            return null;
        }

        /// <summary>
        /// Retrieves an iamge with the contents of the user's entire desktop.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> with an image containg all screens.</returns>
        public static Screenshot FromDesktop()
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.Desktop;

                // Calculate the size of the user's entire desktop.
                int left, top, right, bottom;
                Common.GetDesktopBounds(out left, out top, out right, out bottom);

                //Do the shit
                screenshot.Bitmap = new Bitmap(right - left, bottom - top, DefaultPixelFormat);
                using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                {
                    foreach (Screen s in Screen.AllScreens)
                    {
                        g.CopyFromScreen(s.Bounds.X, s.Bounds.Y, s.Bounds.X + Math.Abs(left), s.Bounds.Y + Math.Abs(top), s.Bounds.Size, CopyPixelOperation.SourceCopy);
                    }
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }
            return null;
        }

        /// <summary>
        /// Retrieves an image with the contents of the active window.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> with the active window capture.</returns>
        public static Screenshot FromActiveWindow()
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.WindowCapture;
                Rectangle rectWindow = Common.GetActiveWindowDimensions();
                Size sizeWindow = new Size(rectWindow.Width, rectWindow.Height);
                screenshot.WindowTitle = Common.GetActiveWindowCaption();
                WriteLine("Found a {0}x{1} window at ({2}, {3}) titled {4}", rectWindow.Width, rectWindow.Height, rectWindow.X, rectWindow.Y, screenshot.WindowTitle);

                screenshot.Bitmap = new Bitmap(rectWindow.Width, rectWindow.Height, DefaultPixelFormat);
                using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                {
                    g.CopyFromScreen(rectWindow.X, rectWindow.Y, 0, 0, sizeWindow, CopyPixelOperation.SourceCopy);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }

            return null;
        }

        /// <summary>
        /// Shows an overlay over the screen that allows the user to select a region, of which
        /// the image is captured and returned.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> with the selected region.</returns>
        public static Screenshot FromRegion()
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.RegionCapture;
                RegionOverlay overlay = new RegionOverlay();
                if (overlay.ShowDialog() == DialogResult.OK)
                {
                    Rectangle rect = overlay.SelectedRegion;
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        WriteLine("Drawn rectangle of {0}x{1} starting at ({1}, {2})", rect.Width, rect.Height, rect.X, rect.Y);

                        screenshot.Bitmap = new Bitmap(rect.Width, rect.Height, DefaultPixelFormat);
                        using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                        {
                            g.CopyFromScreen(rect.X, rect.Y, 0, 0, new Size(rect.Width, rect.Height), CopyPixelOperation.SourceCopy);
                        }
                        return screenshot;
                    }
                    else
                    {
                        WriteLine("Nothing to capture (empty rectangle)", rect.Width, rect.Height);
                    }
                }
                else
                {
                    WriteLine("User cancelled overlay");
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
            }

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="Superscrot.Screenshot"/> instance based on the image data on the clipboard.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> based on the clipboard image.</returns>
        /// <exception cref="System.InvalidOperationException">The clipboard is empty or does not contain an image.</exception>
        public static Screenshot FromClipboard()
        {
            if (!Clipboard.ContainsImage()) throw new InvalidOperationException("The clipboard is empty or does not contain an image.");
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.Clipboard;
                screenshot.Bitmap = (Bitmap)Clipboard.GetImage();
                WriteLine("Clipboard contains a {0}x{1} image", screenshot.Bitmap.Width, screenshot.Bitmap.Height);
                return screenshot;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                return null;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Superscrot.Screenshot"/> instance based on the specified image file.
        /// </summary>
        /// <returns>A <see cref="Superscrot.Screenshot"/> based on the specified image file.</returns>
        public static Screenshot FromFile(string path)
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.File;
                screenshot.OriginalFileName = path;
                screenshot.Bitmap = (Bitmap)Image.FromFile(path);
                WriteLine("{0} is a {1}x{2} image", path, screenshot.Bitmap.Width, screenshot.Bitmap.Height);
                return screenshot;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
                System.Media.SystemSounds.Exclamation.Play();
                return null;
            }
        }

        /// <summary>
        /// Saves this screenshot to the specified stream in an image format based on the current 
        /// program settings.
        /// </summary>
        /// <param name="destination">The <see cref="System.IO.Stream"/> where the image will be saved.</param>
        public void SaveToStream(Stream destination)
        {
            if (this.Bitmap == null) return;

            if (Source == ScreenshotSource.File)
            {
                WriteLine("Using original file \"{0}\"", OriginalFileName);
                using (StreamReader sr = new StreamReader(OriginalFileName))
                {
                    sr.BaseStream.CopyTo(destination);
                }
            }
            else if (Program.Config.UseCompression)
            {
                if (Program.Config.JpegQuality == 0)
                    WriteLine("Using JPEG compression (Sweet Bro and Hella Jeff mode)", Program.Config.JpegQuality);
                else
                    WriteLine("Using JPEG compression (quality level {0})", Program.Config.JpegQuality);
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.MimeType == "image/jpeg")
                    {
                        ici = codec;
                        break;
                    }
                }

                EncoderParameters ep = new EncoderParameters();
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Program.Config.JpegQuality);
                this.Bitmap.Save(destination, ici, ep);
            }
            else
            {
                WriteLine("Using PNG");
                this.Bitmap.Save(destination, ImageFormat.Png);
            }

            //Reset position of the destination stream
            destination.Position = 0;
        }

        /// <summary>
        /// Saves the screenshot to a temporary file and returns the filename. 
        /// If the screenshot originated from a file, that filename is returned
        /// instead and nothing is written to disk.
        /// </summary>
        /// <returns>The filename of the screenshot.</returns>
        public string SaveToFile()
        {
            if (Source == ScreenshotSource.File)
            {
                return OriginalFileName;
            }
            else
            {
                var tempFile = Path.GetTempFileName();
                return SaveToFile(tempFile);
            }
        }

        /// <summary>
        /// Saves the screenshot to a new file with the specified name in an 
        /// image format based on the current program settings.
        /// </summary>
        /// <param name="path">The name of the file to save to.</param>
        /// <returns>The name of the file saved to.</returns>
        public string SaveToFile(string path)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SaveToStream(file);
            }
            return path;
        }

        /// <summary>
        /// Gets a string that contains the filename for this screenshot, formatted using the program settings.
        /// </summary>
        public string GetFileName()
        {
            string windowTitle = Common.RemoveInvalidFilenameChars(WindowTitle);
            string fileName = Common.RemoveInvalidFilenameChars(Path.GetFileNameWithoutExtension(OriginalFileName));

            string formatted = Program.Config.FilenameFormat;
            formatted = formatted.Replace("%c", Common.RemoveInvalidFilenameChars(Environment.MachineName));
            formatted = formatted.Replace("%d", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            formatted = formatted.Replace("%w", Bitmap.Width.ToString());
            formatted = formatted.Replace("%h", Bitmap.Height.ToString());
            formatted = formatted.Replace("%t", windowTitle);
            formatted = formatted.Replace("%f", fileName);

            switch (Source)
            {
                case ScreenshotSource.Desktop:
                    formatted = formatted.Replace("%s", "Desktop");
                    formatted = formatted.Replace("%i", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.Clipboard:
                    formatted = formatted.Replace("%s", "Clipboard");
                    formatted = formatted.Replace("%i", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.RegionCapture:
                    formatted = formatted.Replace("%s", "Capture");
                    formatted = formatted.Replace("%i", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.WindowCapture:
                    formatted = formatted.Replace("%s", "Window");
                    formatted = formatted.Replace("%i", windowTitle);
                    break;
                case ScreenshotSource.File:
                    formatted = formatted.Replace("%s", "File");
                    formatted = formatted.Replace("%i", fileName);
                    break;
            }

            if (Source == ScreenshotSource.File)
                formatted = Path.ChangeExtension(formatted, Path.GetExtension(OriginalFileName));
            else if (Program.Config.UseCompression && !(formatted.EndsWith(".jpg") || formatted.EndsWith(".jpeg")))
                formatted += ".jpg";
            else if (!formatted.EndsWith(".png"))
                formatted += ".png";
            return formatted;
        }
    }
}
