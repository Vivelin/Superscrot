using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;

namespace Superscrot
{
    /// <summary>
    /// Provides common configurable settings.
    /// </summary>
    public class Configuration
    {
        private long _jpegQuality = 90L;
        private double _overlayOpacity = 0.8;

        /// <summary>
        /// Determines whether to display a debug console window.
        /// </summary>
        [DisplayName("Enable developer console"), Category("Debug")]
        [Description("Determines whether to display the developer console window that shows detailed background information.")]
        public bool ConsoleEnabled { get; set; }

        /// <summary>
        /// Determines whether to write console output to a file.
        /// </summary>
        [DisplayName("Enable logging console output to file"), Category("Debug")]
        [Description("Determines whether to write console output to a file.")]
        public bool EnableLogfile { get; set; }

        #region Path settings
        /// <summary>
        /// Gets/sets the path to the directory on the server to save
        /// screenshots to.
        /// </summary>
        [DisplayName("Server path"), Category("FTP settings")]
        [Description("The path to the directory on the server to save screenshots to. If it doesn't exist, it will be created. Example: public_html/screenshots")]
        public string FtpServerPath { get; set; }

        /// <summary>
        /// Gets/sets the base URI that leads to the FTP server path. 
        /// This will be copied to the clipboard after a succesfull upload.
        /// </summary>
        [DisplayName("HTTP base URI"), Category("FTP settings")]
        [Description("The base URI that is copied to the clipboard after uploading. Example: http://www.example.com/screenshots/")]
        public string HttpBaseUri { get; set; }
        #endregion

        #region FTP settings
        /// <summary>
        /// Gets/sets the hostname or IP address of the FTP server to upload to. 
        /// </summary>
        [DisplayName("Hostname"), Category("FTP settings")]
        [Description("The hostname or IP address of the FTP server to upload to. Example: ftp.example.com")]
        public string FtpHostname { get; set; }

        /// <summary>
        /// Gets/sets the port of the FTP server to upload to. 
        /// </summary>
        [DisplayName("Port"), Category("FTP settings")]
        [Description("The port of the FTP server to upload to. Usually 21.")]
        public int FtpPort { get; set; }

        /// <summary>
        /// Gets/sets the username of a user on the FTP server to upload as.
        /// </summary>
        [DisplayName("Username"), Category("FTP settings")]
        [Description("The username of a user on the FTP server to upload as.")]
        public string FtpUsername { get; set; }

        /// <summary>
        /// Gets/sets the password of the user.
        /// </summary>
        [DisplayName("Password"), Category("FTP settings")]
        [Description("All your password are belong to me.")]
        [PasswordPropertyText(true)]
        public string FtpPassword { get; set; }

        /// <summary>
        /// Gets/sets the timeout in milliseconds to wait for responses to FTP requests.
        /// </summary>
        [DisplayName("Timeout"), Category("FTP settings")]
        [Description("The timeout in milliseconds to wait for respones to FTP requests.")]
        public int FtpTimeout { get; set; }

        /// <summary>
        /// Gets/sets a value that determines whether to use SSH FTP.
        /// </summary>
        [DisplayName("Use SSH"), Category("FTP settings")]
        [Description("Determines whether to use SSH FTP")]
        public bool UseSSH { get; set; }
        #endregion

        #region Image settings
        /// <summary>
        /// Gets/sets the format of the filename that screenshots are saved as.
        /// </summary>
        [DisplayName("Filename format"), Category("Image settings")]
        [Description(@"The format of the filename that screenshots are saved as. See http://www.horsedrowner.net/superscrot#filenameformats. The extension is automatically changed or appended before uploading.")]
        public string FilenameFormat { get; set; }

        /// <summary>
        /// Determines whether to use JPEG compression.
        /// </summary>
        [DisplayName("Use JPEG compression"), Category("Image settings")]
        [Description("Determines whether to use JPEG compression for screenshots (high quality, but still JPEG). If disabled, uses PNG.")]
        public bool UseCompression { get; set; }

        /// <summary>
        /// Gets/sets the quality level of JPEG compressed images.
        /// </summary>
        [DisplayName("JPEG quality"), Category("Image settings")]
        [Description("The quality of JPEG compressed images. Supports values 0 - 100. 90 by default.")]
        public long JpegQuality
        {
            get { return _jpegQuality; }
            set
            {
                if (value > 100) _jpegQuality = 100;
                if (value < 0) _jpegQuality = 0;
                else _jpegQuality = value;
            }
        }
        #endregion

        #region Overlay settings
        /// <summary>
        /// Gets/sets the background color on the region selection overlay.
        /// </summary>
        [DisplayName("Background color"), Category("Overlay settings")]
        [Description("The background color on the region selection overlay.")]
        [XmlIgnore()]
        public Color OverlayBackgroundColor { get; set; }

        /// <summary>
        /// I love XML Serialization but this is fucking gay.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("OverlayBackgroundColor")]
        public string XmlOverlayBackgroundColor
        {
            get { return XmlColor.SerializeColor(OverlayBackgroundColor); }
            set { OverlayBackgroundColor = XmlColor.DeserializeColor(value); }
        }

        /// <summary>
        /// Gets/set the foreground color on the region selection overlay.
        /// </summary>
        [DisplayName("Foreground color"), Category("Overlay settings")]
        [Description("The foreground color on the region selection overlay.")]
        [XmlIgnore()]
        public Color OverlayForegroundColor { get; set; }

        /// <summary>
        /// I love XML Serialization but this is fucking gay.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlElement("OverlayForegroundColor")]
        public string XmlOverlayForegroundColor
        {
            get { return XmlColor.SerializeColor(OverlayForegroundColor); }
            set { OverlayForegroundColor = XmlColor.DeserializeColor(value); }
        }

        /// <summary>
        /// Gets/sets the opacity of the overlay (0 - 1).
        /// </summary>
        [TypeConverterAttribute(typeof(System.Windows.Forms.OpacityConverter))]
        [DisplayName("Opacity"), Category("Overlay settings")]
        [Description("The opacity of the overlay (0 - 100). If you set a value of 0 or 100 you'll probably have problems though.")]
        public double OverlayOpacity
        {
            get { return _overlayOpacity; }
            set
            {
                if (value > 1.0)
                    _overlayOpacity = 1.0;
                else if (value < 0.0)
                    _overlayOpacity = 0.0;
                else
                    _overlayOpacity = value;
            }
        }
        #endregion

        /// <summary>
        /// Determines whether to display a system tray icon.
        /// </summary>
        [DisplayName("Enable tray icon"), Category("User interface")]
        [Description("Determines whether to display a system tray icon.")]
        public bool EnableTrayIcon { get; set; }

        /// <summary>
        /// Loads default values.
        /// </summary>
        public Configuration()
        {
            ConsoleEnabled = false;
            EnableLogfile = true;
            EnableTrayIcon = true;

            FilenameFormat = "%c\\%s\\%d-%i";
            FtpPort = 21;
            UseCompression = true;
            JpegQuality = 90L;
            FtpTimeout = 30000;

            OverlayBackgroundColor = Color.Black;
            OverlayForegroundColor = Color.White;
            OverlayOpacity = 0.6;
        }

        /// <summary>
        /// Loads settings from the specified file.
        /// </summary>
        /// <param name="filename">The filename of an XML file that has been serialized by SaveSettings.</param>
        /// <returns>A new instance of the Configuration class.</returns>
        public static Configuration LoadSettings(string filename)
        {
            Configuration config = null;

            try
            {
                XmlSerializer x = new XmlSerializer(typeof(Configuration));
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlReader xr = XmlReader.Create(fs);
                    config = x.Deserialize(xr) as Configuration;
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }

            return config;
        }

        /// <summary>
        /// Saves settings to the specified file.
        /// </summary>
        /// <param name="filename">The filename of the XML file to save to.</param>
        public void SaveSettings(string filename)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                XmlSerializer x = new XmlSerializer(typeof(Configuration));
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    XmlWriter xw = XmlWriter.Create(fs);
                    x.Serialize(xw, this);
                }
            }
            catch (Exception ex)
            {
                Program.ConsoleException(ex);
            }
        }
    }
}
