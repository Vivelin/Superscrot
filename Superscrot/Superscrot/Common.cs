using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
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

        private const int ECM_FIRST = 0x1500;
        internal const int EM_SETCUEBANNER = ECM_FIRST + 1;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

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
        /// <param name="s">The string to URL encode.</param>
        /// <returns>The URL encoded string, or an empty string.</returns>
        public static string UrlEncode(string str)
        {
            string encoded = string.Empty;

            try
            {
                foreach (char c in str)
                {
                    // http://www.blooberry.com/indexdot/html/topics/urlencoding.htm
                    bool isControlChar = ((0x00 < c && c < 0x1F) || (c == 0x7F));
                    bool isNonASCIIChar = (0x80 < c && c < 0xFF);
                    bool isReservedChar = ("$&+,:;=?@".IndexOf(c) >= 0); //Don't encode forward slash
                    bool isUnsafeChar = (" \"<>#%{}|^~[]`".IndexOf(c) >= 0); //Don't encode backslash
                    if (isControlChar || isNonASCIIChar || isReservedChar || isUnsafeChar)
                        encoded += string.Format("%{0:x}", (int)c);
                    else
                        encoded += c;
                }
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
            RECT rect;
            IntPtr handle = GetForegroundWindow();
            if (!GetWindowRect(handle, out rect))
            {
                throw new Exception("GetWindowRect failed");
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
            IntPtr handle = GetForegroundWindow();
            int length = GetWindowTextLength(handle);
            if (length <= 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Sets the textual cue, or tip, that is displayed by the textbox to prompt the user for information.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="cue">A string that contains the text to display as the textual cue.</param>
        public static void SetCue(this TextBox textBox, string cue)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, cue);
        }

        /// <summary>
        /// Clears the textual cue, or tip, that is displayed by the textbox to prompt the user for information.
        /// </summary>
        /// <param name="textBox">The text box</param>
        public static void ClearCue(this TextBox textBox)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, string.Empty);
        }
    }
}
