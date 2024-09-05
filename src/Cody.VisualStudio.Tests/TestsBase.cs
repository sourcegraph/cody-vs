using System;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase
    {
        protected CodyPackage CodyPackage;
        protected IVsUIShell UIShell;
        protected DTE2 Dte;

        //public TestsBase()
        //{
        //    Dte = (DTE2)Package.GetGlobalService(typeof(DTE));
        //    UIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
        //}

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
            
            return codyPackage;
        }

        protected async Task WaitForAsync(Func<bool> condition)
        {
            while (!condition.Invoke())
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                CodyPackage.Logger.Debug($"Chat not loaded ...");
            }

            CodyPackage.Logger.Debug($"Chat loaded.");
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
            bool isChatLoaded = false;
            await CodyPackage.ShowToolWindowAsync();

            var viewModel = CodyPackage.MainViewModel;
            await WaitForAsync(() => viewModel.IsChatLoaded);

            isChatLoaded = viewModel.IsChatLoaded;
            CodyPackage.Logger.Debug($"Chat loaded:{isChatLoaded}");
           
        }
    }
}
