using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace WebView2.VisualStudio
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("4de7d882-dc1c-42ec-a96d-0d0df6ff9b4b")]
    public class WebView2ToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebView2ToolWindow"/> class.
        /// </summary>
        public WebView2ToolWindow() : base(null)
        {
            this.Caption = "WebView2ToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new WebView2ToolWindowControl();
        }
    }
}
