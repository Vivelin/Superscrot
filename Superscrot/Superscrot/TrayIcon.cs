using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Handles the application's tray icon initializes and behaviour. Console output is colored White.
    /// </summary>
    public class TrayIcon
    {
        private static void Write(string text) { Program.ConsoleWrite(ConsoleColor.White, text); }
        private static void Write(string format, params object[] arg) { Program.ConsoleWrite(ConsoleColor.White, format, arg); }
        private static void WriteLine(string text) { Program.ConsoleWriteLine(ConsoleColor.White, text); }
        private static void WriteLine(string format, params object[] arg) { Program.ConsoleWriteLine(ConsoleColor.White, format, arg); }

        private static TrayIcon _instance = null;

        /// <summary>
        /// WELL I WONDER WHAT THIS DOES.
        /// </summary>
        private NotifyIcon Tray { get; set; }

        /// <summary>
        /// Gets a reference to the current tray icon, or null if it is disabled.
        /// </summary>
        public static TrayIcon GetInstance()
        {
            if (Program.Config.EnableTrayIcon)
            {
                if (_instance == null)
                    _instance = new TrayIcon();
                return _instance;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public TrayIcon()
        {
            
        }

        /// <summary>
        /// Releases all resources used by the tray icon.
        /// </summary>
        ~TrayIcon()
        {
            if (Tray != null)
            {
                Tray.Visible = false;
                Tray.Dispose();
                Tray = null;
            }
        }
        
        /// <summary>
        /// Hurp derp durp durp herp.
        /// </summary>
        public void InitializeTrayIcon()
        {
            try
            {
                Tray = new NotifyIcon();
                Tray.Icon = System.Drawing.SystemIcons.Application; //PLACEHOLDER
                Tray.Text = Application.ProductName;
                Tray.ContextMenuStrip = new ContextMenuStrip();
                Tray.ContextMenuStrip.Items.Add("Configure...", Properties.Resources.Configure, new EventHandler(OnTrayConfigure));
                Tray.ContextMenuStrip.Items.Add("Toggle Developer Console", Properties.Resources.Console, new EventHandler(OnTrayShowConsole));
                Tray.ContextMenuStrip.Items.Add("Exit", Properties.Resources.Exit, new EventHandler(OnTrayExit));
                Tray.Visible = true;
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }
        }

        /// <summary>
        /// Hides the tray icon.
        /// </summary>
        public void Hide()
        {
            if (Tray != null) Tray.Visible = false;
        }

        /// <summary>
        /// Changes the tray icon.
        /// </summary>
        /// <param name="icon">The <c>System.Drawing.Icon</c> to set as the new icon.</param>
        public void ChangeIcon(System.Drawing.Icon icon)
        {
            if (icon != null)
            {
                Tray.Icon = icon;
            }
            else
            {
                Tray.Icon = System.Drawing.SystemIcons.Application; //PLACEHOLDER
            }
        }

        /// <summary>
        /// Displays an error message from the tray icon.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="message">The message to display.</param>
        public void ShowError(string title, string message)
        {
            try
            {
                Tray.ShowBalloonTip(10000, title, message, ToolTipIcon.Error);
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }
        }

        /// <summary>
        /// Shows the config editor. (duuuh)
        /// </summary>
        private void OnTrayConfigure(object sender, EventArgs e)
        {
            Program.ShowConfigEditor();
        }

        /// <summary>
        /// Shows or hides the console (again, duh).
        /// </summary>
        private void OnTrayShowConsole(object sender, EventArgs e)
        {
            Program.ToggleConsole();
        }

        /// <summary>
        /// Exits the application when the user clicks Exit.
        /// </summary>
        private void OnTrayExit(object sender, EventArgs e)
        {
            this.Hide();
            Program.Exit();
        }
    }
}
