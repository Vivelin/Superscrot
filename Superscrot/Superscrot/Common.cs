using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Commonly used functions and API calls that have no place elsewhere.
    /// </summary>
    public static class Common
    {
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.DarkGreen, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.DarkGreen, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.DarkGreen, format, arg); }

        /// <summary>
        /// Returns the given input string stripped of invalid filename characters.
        /// </summary>
        /// <param name="input">The string to format.</param>
        public static string RemoveInvalidFilenameChars(string input)
        {
            if (input == null) return string.Empty;

            string formatted = input;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                formatted = formatted.Replace(c, '_');
            }

            return formatted;
        }

        /// <summary>
        /// Combines all parts as an URI, separated by forward slashes. The resulting value does not end with a forward slash.
        /// </summary>
        /// <param name="parts">A collection of strings. Backslashes are converted to forward slashes.</param>
        public static string UriCombine(params string[] parts)
        {
            string combined = string.Empty;

            try
            {
                foreach (string item in parts)
                {
                    if (item != string.Empty)
                    {
                        combined += item.Replace('\\', '/');
                        if (!combined.EndsWith("/")) combined += '/';
                    }
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }

            return combined.TrimEnd('/');
        }

        /// <summary>
        /// Encodes a URL string by replacing certains characters.
        /// </summary>
        /// <param name="str">The string to URL encode.</param>
        /// <returns>The URL encoded string, or an empty string.</returns>
        public static string UrlEncode(string str)
        {
            string encoded = string.Empty;

            try
            {
                string[] paths = str.Split('/', '\\');
                for (int i = 0; i < paths.Length; i++)
                {
                    paths[i] = Uri.EscapeDataString(paths[i]);
                }
                encoded = string.Join("/", paths);
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }

            return encoded;
        }

        /// <summary>
        /// Retrieves the location and size of the active window.
        /// </summary>
        /// <returns>A rectangle with the location and size of the active window.</returns>
        /// <exception cref="System.Exception">Throw if GetWindowRect failed</exception>
        public static Rectangle GetActiveWindowDimensions()
        {
            NativeMethods.RECT rect;
            IntPtr handle = NativeMethods.GetForegroundWindow();
            if (!NativeMethods.GetWindowRect(handle, out rect))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            return new Rectangle(rect.Left, rect.Top, width, height);
        }


        /// <summary>
        /// Returns the coordinates of the left-most, top-most, right-most and bottom-most edges of all screens.
        /// </summary>
        /// <param name="left">The X-coordinate of the left-most edge on the left-most screen.</param>
        /// <param name="top">The Y-coordinate of the top-most edge on the top-most screen.</param>
        /// <param name="right">The X-coordinate of the right-most edge on the right-most screen.</param>
        /// <param name="bottom">The Y-coordinate of the bottom-most edge on the bottom-most screen.</param>
        public static void GetDesktopBounds(out int left, out int top, out int right, out int bottom)
        {
            left = 0;
            top = 0;
            right = 0;
            bottom = 0;
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Bounds.Left < left) left = s.Bounds.Left;
                if (s.Bounds.Top < top) top = s.Bounds.Top;
                if (s.Bounds.Right > right) right = s.Bounds.Right;
                if (s.Bounds.Bottom > bottom) bottom = s.Bounds.Bottom;
            }
        }

        /// <summary>
        /// Retrieves the caption of the active window.
        /// </summary>
        /// <returns>The caption of the active window, or null.</returns>
        public static string GetActiveWindowCaption()
        {
            IntPtr handle = NativeMethods.GetForegroundWindow();
            int length = NativeMethods.GetWindowTextLength(handle);
            if (length <= 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Converts a numeric value into a string that represents the number expressed as a size 
        /// value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
        /// </summary>
        /// <param name="size">The numeric value to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string FormatFileSize(long size)
        {
            var buffer = new StringBuilder(11);
            NativeMethods.StrFormatByteSize(size, buffer, buffer.Capacity);
            return buffer.ToString();
        }

        /// <summary>
        /// Translates a path on the FTP server to an HTTP link.
        /// </summary>
        /// <param name="serverPath">The full path to the file on the server.</param>
        /// <returns>A string that contains the HTTP address of the file.</returns>
        public static string TranslateServerPath(string serverPath)
        {
            if (serverPath == null) return null;
            if (serverPath == string.Empty) return string.Empty;

            var folder = Path.GetDirectoryName(serverPath).Replace('\\', '/') + '/'; // GetDirectoryName never ends in a trailing slash, but FtpServerPath always ends in a trailing slash
            var file = Path.GetFileName(serverPath);

            if (!folder.StartsWith(Program.Config.FtpServerPath))
                throw new Exception("Server path is outside of the configured base server path, " + Program.Config.FtpServerPath);

            var baseUrl = folder.Replace(Program.Config.FtpServerPath, Program.Config.HttpBaseUri);
            var url = UriCombine(baseUrl, UrlEncode(file));
            return url;
        }
    }
}
