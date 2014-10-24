﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Superscrot
{
    /// <summary>
    /// Represents a native window.
    /// </summary>
    internal class NativeWindow
    {
        private NativeMethods.RECT rect;
        private NativeMethods.WINDOWPLACEMENT placement;
        private NativeMethods.WINDOWINFO info;
        private Point? location;
        private Size? size;
        private string caption;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeWindow"/> class
        /// for the specified window handle.
        /// </summary>
        /// <param name="handle">The handle to the window.</param>
        public NativeWindow(IntPtr handle)
        {
            Handle = handle;

            Init();
        }

        /// <summary>
        /// Gets a handle to the window.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Gets the current upper-left coordinate of the window.
        /// </summary>
        public Point Location
        {
            get
            {
                if (!location.HasValue)
                {
                    var left = rect.Left;
                    var top = rect.Top;

                    if (Maximized)
                    {
                        left += (int)info.cxWindowBorders;
                        top += (int)info.cyWindowBorders;
                    }

                    location = new Point(left, top);
                }
                return location.Value;
            }
        }

        /// <summary>
        /// Gets the size of the window.
        /// </summary>
        public Size Size
        {
            get
            {
                if (!size.HasValue)
                {
                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;

                    if (Maximized)
                    {
                        width -= (int)(2 * info.cxWindowBorders);
                        height -= (int)(2 * info.cyWindowBorders);
                    }

                    size = new Size(width, height);
                }
                return size.Value;
            }
        }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public int Width
        {
            get { return Size.Width; }
        }

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        public int Height
        {
            get { return Size.Height; }
        }

        /// <summary>
        /// Determines whether the window is being displayed as a maximized 
        /// window or not.
        /// </summary>
        public bool Maximized
        {
            get { return placement.showCmd == NativeMethods.SHOWCMD.SW_SHOWMAXIMIZED; }
        }

        /// <summary>
        /// Returns the text of the window's title bar.
        /// </summary>
        public string Caption
        {
            get { return caption; }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NativeWindow"/> class for 
        /// the foreground window.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="NativeWindow"/> class, or null in
        /// certain circumstances, such as when a window is losing activation.
        /// </returns>
        public static NativeWindow ForegroundWindow()
        {
            var handle = NativeMethods.GetForegroundWindow();
            return new NativeWindow(handle);
        }

        private void Init()
        {
            /* Make a call to the window manager to get the proper window 
             * dimensions. GetWindowRect doesn't return an accurate rectangle
             * on certain window types */
            var hresult = NativeMethods.DwmGetWindowAttribute(Handle,
                NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                out rect,
                (uint)Marshal.SizeOf(rect));
            Marshal.ThrowExceptionForHR(hresult);

            // Get the window placement for the window state
            placement = new NativeMethods.WINDOWPLACEMENT();
            placement.length = (uint)Marshal.SizeOf(placement);
            if (!NativeMethods.GetWindowPlacement(Handle, out placement))
                throw new Win32Exception();

            /* Sadly, DwmGetWindowAttribute still doesn't return an 
             * accurate rectangle if the window is maximized. Or more 
             * precisely, it is TOO accurate, as the window's borders are
             * included but not visible on the screen... */
            info = new NativeMethods.WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (!NativeMethods.GetWindowInfo(Handle, ref info))
                throw new Win32Exception();

            var length = NativeMethods.GetWindowTextLength(Handle);
            if (length > 0)
            {
                var sb = new StringBuilder(length + 1); // Explicitly null-terminated
                NativeMethods.GetWindowText(Handle, sb, sb.Capacity);
                caption = sb.ToString();
            }
            else
            {
                caption = string.Empty;
            }
        }
    }
}