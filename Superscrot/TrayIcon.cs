using System;
using System.Drawing;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Handles the application's tray icon initializes and behaviour.
    /// </summary>
    public class TrayIcon : IDisposable
    {
        private static TrayIcon _instance = null;

        private ToolStripItem toggleEnableItem;
        private NotifyIcon trayIcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIcon"/> class.
        /// </summary>
        public TrayIcon()
        {
            Program.Manager.EnabledChanged += Manager_EnabledChanged;

            toggleEnableItem = new ToolStripMenuItem("Suspend",
                Properties.Resources.Pause, new EventHandler(OnTrayDisable));

            trayIcon = new NotifyIcon();
            trayIcon.Text = $"{Application.ProductName} {Application.ProductVersion}";
            SetIcon(Properties.Resources.IconImage);

            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add(toggleEnableItem);
            trayIcon.ContextMenuStrip.Items.Add("-");
            trayIcon.ContextMenuStrip.Items.Add("Settings", Properties.Resources.Configure, new EventHandler(OnTrayConfigure));
            trayIcon.ContextMenuStrip.Items.Add("Exit", Properties.Resources.Exit, new EventHandler(OnTrayExit));
        }

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
        /// Releases resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Hides the tray icon.
        /// </summary>
        public void Hide()
        {
            trayIcon.Visible = false;
        }

        /// <summary>
        /// Shows the tray icon.
        /// </summary>
        public void Show()
        {
            trayIcon.Visible = true;
        }

        /// <summary>
        /// Displays an error message from the tray icon.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="message">The message to display.</param>
        public void ShowError(string title, string message)
        {
            ShowMessage(title, message, ToolTipIcon.Error);
        }

        /// <summary>
        /// Displays a message from the tray icon.
        /// </summary>
        /// <param name="title">The title of the message to display.</param>
        /// <param name="message">The message contents to display.</param>
        /// <param name="icon">The icon to display with the message.</param>
        public void ShowMessage(string title, string message, System.Windows.Forms.ToolTipIcon icon = ToolTipIcon.None)
        {
            trayIcon.ShowBalloonTip(10000, title, message, icon);
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        /// <param name="disposing">True to release managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null;
                }
                if (toggleEnableItem != null)
                {
                    toggleEnableItem.Dispose();
                    toggleEnableItem = null;
                }
            }
        }

        /// <summary>
        /// Draws the specified <see cref="Image"/> over the tray icon's image.
        /// </summary>
        /// <param name="overlayImage">
        /// An <see cref="Image"/> to draw on top of the tray icon's image.
        /// </param>
        protected void DrawOverlay(Image overlayImage)
        {
            var image = Properties.Resources.IconImage;
            using (var g = Graphics.FromImage(image))
            {
                g.DrawImage(overlayImage, 0, 0);
            }

            SetIcon(image);
        }

        /// <summary>
        /// Shows the config editor.
        /// </summary>
        protected virtual void OnTrayConfigure(object sender, EventArgs e)
        {
            Program.ShowConfigEditor();
        }

        /// <summary>
        /// Suspends or resumes Superscrot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTrayDisable(object sender, EventArgs e)
        {
            Program.Manager.Enabled = !Program.Manager.Enabled;
        }

        /// <summary>
        /// Exits the application when the user clicks Exit.
        /// </summary>
        protected virtual void OnTrayExit(object sender, EventArgs e)
        {
            this.Hide();
            Program.Exit();
        }

        /// <summary>
        /// Sets the tray icon's image to the specified <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="image">A <see cref="Bitmap"/> image to set as icon.</param>
        protected void SetIcon(Bitmap image)
        {
            trayIcon.Icon = Icon.FromHandle(image.GetHicon());
        }

        private void Manager_EnabledChanged(object sender, EventArgs e)
        {
            if (Program.Manager.Enabled)
            {
                toggleEnableItem.Text = "Suspend";
                toggleEnableItem.Image = Properties.Resources.Pause;
                SetIcon(Properties.Resources.IconImage);
            }
            else
            {
                toggleEnableItem.Text = "Resume";
                toggleEnableItem.Image = Properties.Resources.Start;
                DrawOverlay(Properties.Resources.StoppedOverlay);
            }
        }
    }
}
