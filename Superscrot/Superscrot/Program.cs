using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Superscrot
{
    /// <summary>
    /// Handles program initialization and console behaviour. Console output is colored White.
    /// </summary>
    public static class Program
    {
        private static bool _startedWithDefaultSettings = false;

        private static EventWaitHandle _startupEventHandle;

        /// <summary>
        /// Occurs when the <see cref="Config"/> property changes.
        /// </summary>
        public static event EventHandler ConfigurationChanged;

        private static string _settingsPath = string.Empty;
        /// <summary>
        /// Gets the path to the file where the settings are located.
        /// </summary>
        public static string SettingsPath
        {
            get { return _settingsPath; }
        }

        private static Configuration _config = null;
        /// <summary>
        /// Provides common configurable settings.
        /// </summary>
        public static Configuration Config
        {
            get { return _config; }
            internal set 
            {
                if (value != _config)
                {
                    _config = value;
                    OnConfigurationChanged();
                }
            }
        }				

        private static Manager _manager = null;

        /// <summary>
        /// Coordinates top-level functionality and provides common functions that interact between 
        /// classes.
        /// </summary>
        public static Manager Manager
        {
            get { return _manager; }
            internal set { _manager = value; }
        }

        /// <summary>
        /// This application's tray icon.
        /// </summary>
        public static TrayIcon Tray
        {
            get { return TrayIcon.GetInstance(); }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            Manager = new Superscrot.Manager();
            LoadSettings();

            CommandlineParser cmd = new CommandlineParser(args);
            if (cmd["now"] != null)
            {
                Manager.TakeAndUploadRegionScreenshot();
                return;
            }

            if (_startedWithDefaultSettings || cmd["config"] != null) ShowConfigEditor();

            bool created = false;
            _startupEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset, Environment.UserName + "SuperscrotStartup", out created);
            if (created)
            {
                if (!Manager.InitializeKeyboardHook())
                {
                    Exit();
                    return;
                }
                Tray.Show();
                Application.Run();
            }
            else
            {
                MessageBox.Show("Superscrot is already running. You may configure Superscrot by using /config, but you will need to restart the running instance.", "Superscrot", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
        }

        /// <summary>
        /// Cleans up and exit the application.
        /// </summary>
        public static void Exit()
        {
            Tray.Dispose();
            Manager.Dispose();
            NativeMethods.FreeConsole();
            Application.Exit();
        }

        /// <summary>
        /// Starts the config editor.
        /// </summary>
        public static void ShowConfigEditor()
        {
            using (var settings = new Dialogs.Settings())
            {
                settings.Configuration = new Configuration(Program.Config);
                settings.ShowDialog();
            }
        }

        /// <summary>
        /// Raises the <see cref="ConfigurationChanged"/> event.
        /// </summary>
        private static void OnConfigurationChanged()
        {
            var handler = ConfigurationChanged;
            if (handler != null)
                handler(null, EventArgs.Empty);
        }

        /// <summary>
        /// Logs unhandled exceptions as fatal exceptions.
        /// </summary>
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                Trace.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("An exception occurred while handling an unhandled exception:");
                Trace.WriteLine(ex);
            }
        }

        private static void LoadSettings()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Superscrot");
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            _settingsPath = Path.Combine(appData, "Config.xml");

            var logName = string.Format("{0:y}.svclog", DateTime.Now);
            var logPath = Path.Combine(appData, logName);
            Trace.Listeners.Add(new XmlWriterTraceListener(logPath));
            Trace.AutoFlush = true;

            if (File.Exists(_settingsPath))
            {
                _config = Configuration.LoadSettings(_settingsPath);
            }
            else
            {
                //Save default settings
                _config = new Configuration();
                Program.Config.SaveSettings(_settingsPath);
                _startedWithDefaultSettings = true;
            }
        }
    }
}
