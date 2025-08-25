using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Cody.UI.Views
{
    /// <summary>
    /// Interaction logic for ToastView.xaml
    /// </summary>
    public partial class ToastView : Window
    {
        private const int windowMargin = 30;

        public ToastView()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.Owner != null)
            {
                this.Owner.LocationChanged += OnOwnerLocationChanged;
                this.Owner.SizeChanged += OnOwnerSizeChanged;
                this.Owner.StateChanged += OnOwnerStateChanged;
            }
        }

        private void OnOwnerStateChanged(object sender, EventArgs e) => PositionWindow();

        private void OnOwnerLocationChanged(object sender, EventArgs e) => PositionWindow();

        private void OnOwnerSizeChanged(object sender, SizeChangedEventArgs e) => PositionWindow();

        public void PositionWindow()
        {
            if (this.Owner == null) return;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                var ownerBounds = Win32Bounds.GetOwnerAnchorRect(this);

                this.Left = ownerBounds.Right - this.ActualWidth - windowMargin;
                this.Top = ownerBounds.Bottom - this.ActualHeight - windowMargin;
            }
            else
            {
                this.Left = this.Owner.Left + this.Owner.ActualWidth - this.ActualWidth - windowMargin;
                this.Top = this.Owner.Top + this.Owner.ActualHeight - this.ActualHeight - windowMargin;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.Owner != null)
            {
                this.Owner.LocationChanged -= OnOwnerLocationChanged;
                this.Owner.SizeChanged -= OnOwnerSizeChanged;
                this.Owner.StateChanged -= OnOwnerStateChanged;
            }
            base.OnClosed(e);
        }
    }


    static class Win32Bounds
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor; // entire monitor (device px)
            public RECT rcWork;    // working area (device px, excludes taskbar)
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        public static Rect GetOwnerAnchorRect(Window owner)
        {
            var hwnd = new WindowInteropHelper(owner).Handle;
            var hmon = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hmon, ref mi))
                return DeviceRectToDips(mi.rcWork, hwnd);

            return new Rect();
        }

        private static Rect DeviceRectToDips(RECT r, IntPtr hwnd)
        {
            var src = HwndSource.FromHwnd(hwnd);
            var fromDevice = src.CompositionTarget.TransformFromDevice;
            var tl = fromDevice.Transform(new Point(r.Left, r.Top));
            var br = fromDevice.Transform(new Point(r.Right, r.Bottom));
            return new Rect(tl, br);
        }
    }
}
