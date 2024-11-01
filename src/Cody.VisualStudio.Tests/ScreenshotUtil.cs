using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Cody.VisualStudio.Tests
{
    public class ScreenshotUtil
    {
        public static void CaptureWindow(IntPtr hwnd, string path)
        {
            var rect = default(RECT);
            GetWindowRect(hwnd, ref rect);
            CaptureScreenArea(
                path,
                left: rect.Left,
                top: rect.Top,
                width: rect.Right - rect.Left,
                height: rect.Bottom - rect.Top);
        }

        public static void CaptureScreenArea(string path, int left, int top, int width, int height)
        {
            using (var bitmap = new Bitmap(width, height))
            using (var image = Graphics.FromImage(bitmap))
            {
                image.CopyFromScreen(
                    sourceX: left,
                    sourceY: top,
                    blockRegionSize: new Size(width, height),
                    copyPixelOperation: CopyPixelOperation.SourceCopy,
                    destinationX: 0,
                    destinationY: 0);

                bitmap.Save(path, ImageFormat.Png);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }

}
