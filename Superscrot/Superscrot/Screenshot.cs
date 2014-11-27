using System;
using System.Collections.Specialized;
using System.Diagnostics;
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
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.DarkGreen, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.DarkGreen, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, format, arg); }

        /// <summary>
        /// #0D0B0C, a color that Windows doesn't seem to like very much.
        /// </summary>
        protected static readonly Color ThatFuckingColor = 
            Color.FromArgb(0xFF, 0x0D, 0x0B, 0x0C);

        private Bitmap bitmap;
        private string serverPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Screenshot"/> class.
        /// </summary>
        public Screenshot() { }

        /// <summary>
        /// Occurs when the screenshot has been uploaded or the path on the 
        /// server has changed.
        /// </summary>
        public event EventHandler Uploaded;

        /// <summary>
        /// Gets or sets the source of the screenshot.
        /// </summary>
        public ScreenshotSource Source { get; set; }

        /// <summary>
        /// Gets or sets a bitmap image of the screenshot.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        /// <summary>
        /// Gets or sets the path on the server, or <c>null</c> if the 
        /// screenshot hasn't been uploaded yet.
        /// </summary>
        public string ServerPath
        {
            get { return serverPath; }
            set
            {
                if (value != serverPath)
                {
                    serverPath = value;
                    PublicUrl = PathUtility.TranslateServerPath(value);
                    OnUploaded();
                }
            }
        }

        /// <summary>
        /// Gets the public URL to the file on the server, or <c>null</c> if 
        /// the screenshot hasn't been uploaded yet.
        /// </summary>
        public string PublicUrl { get; private set; }

        /// <summary>
        /// Gets or sets the title of the window the screenshot was taken of, 
        /// or <c>null</c> for non-window captures.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Gets or sets a string that represents the owner of the window the 
        /// screenshot was taken of, or <c>null</c>.
        /// </summary>
        public string WindowOwner { get; set; }

        /// <summary>
        /// Gets or sets the original filename that the screenshot originates 
        /// from, or <c>null</c> for non file-based captures.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Releases all resources used by the <see cref="Screenshot"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Screenshot"/> class.
        /// </summary>
        /// <param name="disposing">True to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }

        /// <summary>
        /// Retrieves an image containing a screenshot of the user's entire 
        /// desktop.
        /// </summary>
        /// <returns>A new <see cref="Screenshot"/> with an image containg a 
        /// screenshot of all screens combined.</returns>
        public static Screenshot FromDesktop()
        {
            try
            {
                var screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.Desktop;

                var bounds = GetDesktopBounds();
                screenshot.Bitmap = new Bitmap(bounds.Width, bounds.Height);
                using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                {
                    foreach (var screen in Screen.AllScreens)
                    {
                        var destination = new Point(
                            screen.Bounds.X + Math.Abs(bounds.Left),
                            screen.Bounds.Y + Math.Abs(bounds.Top));
                        using (var screenBitmap = CopyFromScreen(screen))
                        {
                            g.DrawImageUnscaled(screenBitmap, destination);
                        }
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
        /// <returns>A <see cref="Screenshot"/> with the active window capture.</returns>
        public static Screenshot FromActiveWindow()
        {
            try
            {
                Screenshot screenshot = new Screenshot();
                screenshot.Source = ScreenshotSource.WindowCapture;

                using (var window = NativeWindow.ForegroundWindow())
                {
                    screenshot.WindowTitle = window.Caption;
                    screenshot.WindowOwner = window.Owner.MainModule.FileVersionInfo.FileDescription;

                    WriteLine("Found a {0} window at {1} titled {2}",
                        window.Size, window.Location, window.Caption);

                    screenshot.Bitmap = new Bitmap(window.Width, window.Height);
                    using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                    {
                        g.Clear(ThatFuckingColor);
                        g.CopyFromScreen(window.Location, Point.Empty,
                            window.Size, CopyPixelOperation.SourceCopy);
                    }
                    return screenshot;
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
        /// Shows an overlay over the screen that allows the user to select a 
        /// region, of which the image is captured and returned.
        /// </summary>
        /// <returns>A <see cref="Screenshot"/> with the selected region.</returns>
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

                        screenshot.Bitmap = new Bitmap(rect.Width, rect.Height);
                        using (Graphics g = Graphics.FromImage(screenshot.Bitmap))
                        {
                            g.Clear(ThatFuckingColor);
                            g.CopyFromScreen(rect.X, rect.Y, 0, 0, 
                                new Size(rect.Width, rect.Height), 
                                CopyPixelOperation.SourceCopy);
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
        /// Gets a string that contains the filename for this screenshot, 
        /// formatted using the program settings.
        /// </summary>
        public string GetFileName()
        {
            return GetFileName(Program.Config.FilenameFormat);
        }

        /// <summary>
        /// Gets a string that contains the filename for this screenshot, 
        /// formatted using the specified format string.
        /// </summary>
        /// <param name="format">The composite format string.</param>
        public string GetFileName(string format)
        {
            var time = DateTime.Now;
            var fileName = Path.GetFileNameWithoutExtension(OriginalFileName);
            var args = new StringDictionary();
            args.Add("machine", Environment.MachineName);
            args.Add("width", Bitmap.Width.ToString());
            args.Add("height", Bitmap.Height.ToString());
            args.Add("window", WindowTitle);
            args.Add("process", WindowOwner);
            args.Add("file", fileName);
            
            // Date/time related placeholders
            args.Add("time", time.ToString("yyyyMMddHHmmssffff"));
            args.Add("unix", time.ToUnixTimestamp().ToString());

            switch (Source)
            {
                case ScreenshotSource.Desktop:
                    args.Add("source", "Desktop");
                    args.Add("name", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.Clipboard:
                    args.Add("source", "Clipboard");
                    args.Add("name", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.RegionCapture:
                    args.Add("source", "Capture");
                    args.Add("name", Bitmap.Width.ToString() + "x" + Bitmap.Height.ToString());
                    break;
                case ScreenshotSource.WindowCapture:
                    args.Add("source", "Window");
                    args.Add("name", WindowOwner);
                    break;
                case ScreenshotSource.File:
                    args.Add("source", "File");
                    args.Add("name", fileName);
                    break;
            }

            var result = PathUtility.Format(format, args, time);
            var ext = Program.Config.UseCompression ? "jpg" : "png";
            if (Source == ScreenshotSource.File)
                ext = Path.GetExtension(OriginalFileName);
            return Path.ChangeExtension(result, ext);
        }

        /// <summary>
        /// Calculates the size (in bytes) of the screenshot.
        /// </summary>
        /// <returns>The size of the screenshot, in bytes.</returns>
        public long CalculateSize()
        {
            if (File.Exists(OriginalFileName))
            {
                var info = new FileInfo(OriginalFileName);
                return info.Length;
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    SaveToStream(stream);
                    return stream.Length;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="Bitmap"/> containing an image of the specified
        /// screen.
        /// </summary>
        /// <param name="screen">The <see cref="Screen"/> to capture.</param>
        /// <returns>A new <see cref="Bitmap"/> object representing <paramref 
        /// name="screen"/>.</returns>
        protected static Bitmap CopyFromScreen(Screen screen)
        {
            var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(ThatFuckingColor);
                g.CopyFromScreen(screen.Bounds.Location, Point.Empty,
                    screen.Bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        /// <summary>
        /// Returns the coordinates of the left-most, top-most, right-most and 
        /// bottom-most edges of all screens.
        /// </summary>
        protected static Rectangle GetDesktopBounds()
        {
            var left = 0;
            var top = 0;
            var right = 0;
            var bottom = 0;

            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Bounds.Left < left) left = s.Bounds.Left;
                if (s.Bounds.Top < top) top = s.Bounds.Top;
                if (s.Bounds.Right > right) right = s.Bounds.Right;
                if (s.Bounds.Bottom > bottom) bottom = s.Bounds.Bottom;
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private void OnUploaded()
        {
            var handler = Uploaded;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
