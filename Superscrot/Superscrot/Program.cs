using System;
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
        private const int LOGFILE_MAXSIZE = 1024 * 1024 * 1024; //1 GB
        private static string _logPath = string.Empty;
        private static bool _startedWithDefaultSettings = false;

        private static EventWaitHandle _startupEventHandle;

        /// <summary>
        /// Occurs when the <see cref="Config"/> property changes.
        /// </summary>
        public static event EventHandler ConfigurationChanged;

        /// <summary>
        /// Gets whether the application is shutting down.
        /// </summary>
        public static bool IsShuttingDown { get; set; }

        private static bool _consoleVisible = false;
        /// <summary>
        /// Gets/sets whether the developer console is visible.
        /// </summary>
        public static bool ConsoleVisible
        {
            get { return _consoleVisible; }
            set
            {
                IntPtr hWnd = NativeMethods.GetConsoleWindow();
                _consoleVisible = value;

                if (value)
                {
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOWNORMAL);
                }
                else
                {
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_HIDE);
                }
            }
        }

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

        private static StreamWriter _logWriter = null;
        /// <summary>
        /// Gets a reference to the log file writer.
        /// </summary>
        public static StreamWriter Logfile
        {
            get
            {
                if (_logWriter == null && !IsShuttingDown)
                {
                    _logWriter = InitializeLogWriter();
                }
                return _logWriter;
            }
            private set { _logWriter = value; }
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
            NativeMethods.AllocConsole();
            Console.Title = "superscrot Developer Console";
            ConsoleWriteLine(ConsoleColor.Gray, "superscrot " + Application.ProductVersion);

            Manager = new Superscrot.Manager();
            LoadSettings();

            CommandlineParser cmd = new CommandlineParser(args);
            if (cmd["now"] != null)
            {
                ConsoleVisible = false;
                Manager.TakeAndUploadRegionScreenshot();
                return;
            }

            if (cmd["console"] != null) ConsoleVisible = true;
            else if (cmd["no-console"] == null) ConsoleVisible = Program.Config.ConsoleEnabled;

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
                ConsoleWriteLine(ConsoleColor.Gray, "Do not exit the console by closing the window! Use the tray menu option!");
                Application.Run();
            }
            else
            {
                ConsoleWriteLine(ConsoleColor.Gray, "I should go.");
                MessageBox.Show("Superscrot is already running. You may configure Superscrot by using /config, but you will need to restart the running instance.", "Superscrot", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
        }

        /// <summary>
        /// Cleans up and exit the application.
        /// </summary>
        public static void Exit()
        {
            IsShuttingDown = true;
            ConsoleWriteLine(ConsoleColor.Gray, "Cave Johnson, we're done here.");

            if (Logfile != null)
            {
                Logfile.Close();
                Logfile.Dispose();
                Logfile = null;
            }

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
            ConsoleWriteLine(ConsoleColor.Gray, "Switched configuration");

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
                ConsoleFatal(ex);
            }
            catch (Exception ex)
            {
                ConsoleWriteLine(ConsoleColor.Red, "An exception occurred while handling an unhandled exception (" + ex.Message + ")");
            }
        }

        private static void LoadSettings()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Superscrot");
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            _settingsPath = Path.Combine(appData, "Config.xml");
            _logPath = Path.Combine(appData, "Console.log");
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

        /// <summary>
        /// Initializes a new streamwriter for the application's log file. 
        /// If the previous file exceeds a predefined maximum size, it will
        /// be deleted.
        /// </summary>
        /// <returns>A <see cref="System.IO.StreamWriter"/> for the logfile.</returns>
        private static StreamWriter InitializeLogWriter()
        {
            StreamWriter ret = null;

            try
            {
                if (File.Exists(_logPath))
                {
                    FileInfo fi = new FileInfo(_logPath);
                    if (fi.Length > LOGFILE_MAXSIZE)
                    {
                        ConsoleWriteLine(ConsoleColor.White, "Logfile exceeds " + (LOGFILE_MAXSIZE / 1024) + " Mb, deleting");
                        File.Delete(_logPath);
                    }
                }

                ret = new StreamWriter(_logPath, true);
                ret.AutoFlush = true;
            }
            catch (Exception ex)
            {
                ConsoleException(ex);
            }

            return ret;
        }

        /// <summary>
        /// Toggles console visibility.
        /// </summary>
        public static void ToggleConsole()
        {
            ConsoleVisible = !ConsoleVisible;
        }

        /// <summary>
        /// Writes the specified text to the console in the specified color.
        /// </summary>
        /// <param name="color">The foreground color of the text to display.</param>
        /// <param name="text">The text to display.</param>
        public static void ConsoleWrite(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified text to the console in the specified color.
        /// </summary>
        /// <param name="color">The foreground color of the text to display.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format"/>.</param>
        public static void ConsoleWrite(ConsoleColor color, string format, params object[] arg)
        {
            Console.ForegroundColor = color;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified text with a trailing newline to the console in the specified color.
        /// </summary>
        /// <param name="color">The foreground color of the text to display.</param>
        /// <param name="text">The text to display.</param>
        public static void ConsoleWriteLine(ConsoleColor color, string text)
        {
            ConsoleWrite(color, text + Environment.NewLine);
        }

        /// <summary>
        /// Writes the specified text with a trailing newline to the console in the specified color.
        /// </summary>
        /// <param name="color">The foreground color of the text to display.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format"/>.</param>
        public static void ConsoleWriteLine(ConsoleColor color, string format, params object[] arg)
        {
            ConsoleWrite(color, format + Environment.NewLine, arg);
        }

        /// <summary>
        /// Writes the specified Exception in yellow text to the console.
        /// </summary>
        public static void ConsoleException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(ex);
            Console.ResetColor();
            if (Program.Config.EnableLogfile)
            {
                Logfile.Write(DateTime.Now.ToString("yyyyMMddHHmmss") + '\t');
                Logfile.WriteLine(ex);
            }
        }

        /// <summary>
        /// Writes the specified Exception in red text to the console.
        /// </summary>
        public static void ConsoleFatal(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
            if (Program.Config.EnableLogfile)
            {
                Logfile.Write(DateTime.Now.ToString("yyyyMMddHHmmss") + "\t[FATAL]\t");
                Logfile.WriteLine(ex);
            }
        }
    }
}
