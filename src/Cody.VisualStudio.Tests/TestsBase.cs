using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Cody.Core.Logging;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Xunit.Abstractions;
using Thread = System.Threading.Thread;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase: ITestLogger
    {
        private readonly ITestOutputHelper _logger;

        protected CodyPackage CodyPackage;

        protected TestsBase(ITestOutputHelper output)
        {
            _logger = output;

            WriteLog("[TestBase] Initialized.");
        }

        public void WriteLog(string message, string type = "", [CallerMemberName] string callerName = "")
        {
            _logger.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{type}] [{callerName}] [ThreadId:{Thread.CurrentThread.ManagedThreadId}] {message}");
        }

        private IVsUIShell _uiShell;
        protected IVsUIShell UIShell => _uiShell ?? (_uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell)));

        private DTE2 _dte;
        protected DTE2 Dte => _dte ?? (_dte = (DTE2)Package.GetGlobalService(typeof(DTE)));

        protected void OpenSolution(string path) => Dte.Solution.Open(path);

        protected void CloseSolution() => Dte.Solution.Close();

        protected async Task OpenDocument(string path, int? selectLineStart = null, int? selectLineEnd = null)
        {
            VsShellUtilities.OpenDocument(CodyPackage, path, Guid.Empty, out _, out _, out IVsWindowFrame frame);
            frame.Show();

            if (selectLineStart.HasValue && selectLineEnd.HasValue)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar);
                IVsTextView textView = pvar as IVsTextView;
                if (textView == null && pvar is IVsCodeWindow vsCodeWindow)
                {
                    if (vsCodeWindow.GetPrimaryView(out textView) != 0)
                        vsCodeWindow.GetSecondaryView(out textView);
                }

                textView.SetSelection(selectLineStart.Value - 1, 0, selectLineEnd.Value, 0);
                await Task.Delay(500);
            }
        }

        protected async Task OpenCodyChatToolWindow()
        {
            var guid = new Guid(Guids.CodyChatToolWindowString);
            UIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFrameOnly, ref guid, out IVsWindowFrame windowFrame);

            windowFrame.Show();

            await WaitForAsync(() => CodyPackage.MainViewModel.IsChatLoaded);
        }

        protected async Task CloseCodyChatToolWindow()
        {
            var guid = new Guid(Guids.CodyChatToolWindowString);
            UIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFrameOnly, ref guid, out IVsWindowFrame windowFrame);
            windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
            await Task.Delay(500);
        }

        protected bool IsCodyChatToolWindowOpen()
        {
            var guid = new Guid(Guids.CodyChatToolWindowString);
            UIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFrameOnly, ref guid, out IVsWindowFrame windowFrame);
            
            return windowFrame.IsVisible() == 0;
        }

        protected async Task<CodyPackage> GetPackageAsync()
        {
            var guid = CodyPackage.PackageGuidString;
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            CodyPackage = codyPackage;
            var logger = (Logger)CodyPackage.Logger;
            logger.WithTestLogger(this);

            return codyPackage;
        }

        protected async Task WaitForAsync(Func<bool> condition)
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMinutes(2);
            while (!condition.Invoke())
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                WriteLog("Chat not loaded ...");

                var nowTime = DateTime.Now;
                var currentSpan = nowTime - startTime;
                if (currentSpan >= timeout)
                {
                    var message = $"Chat timeout! It's loading for more than {currentSpan.TotalSeconds} s.";
                    WriteLog(message);
                    throw new Exception(message);
                }
            }

            if (condition.Invoke())
            {
                WriteLog($"Condition meet.");
            }
        }

        protected async Task OnUIThread(Func<Task> task)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await task();
            });
        }

        protected async Task WaitForChat()
        {
            WriteLog("Waiting for the Chat ...");

            bool isChatLoaded = false;
            await CodyPackage.ShowToolWindowAsync();
            WriteLog("ShowToolWindowAsync called.");

            var viewModel = CodyPackage.MainViewModel;
            await WaitForAsync(() => viewModel.IsChatLoaded);

            isChatLoaded = viewModel.IsChatLoaded;
            WriteLog($"Chat loaded:{isChatLoaded}");
        }
    }
}
