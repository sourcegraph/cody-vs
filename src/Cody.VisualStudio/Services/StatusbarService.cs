using Cody.Core.Infrastructure;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class StatusbarService : IStatusbarService
    {
        private static bool animationIsRunning = false;
        private object icon = (short)Constants.SBAI_General;

        public void SetText(string text)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var statusBar = (IVsStatusbar)Package.GetGlobalService(typeof(SVsStatusbar));
                int frozen;

                statusBar.IsFrozen(out frozen);

                if (frozen != 0) statusBar.FreezeOutput(0);

                statusBar.SetText(text);
                return Task.CompletedTask;
            });


        }

        public void StartProgressAnimation()
        {
            if (animationIsRunning) return;
            if (EnableProgressAnimation(true)) animationIsRunning = true;
        }

        public void StopProgressAnimation()
        {
            if (!animationIsRunning) return;
            if (EnableProgressAnimation(false)) animationIsRunning = false;
        }

        private bool EnableProgressAnimation(bool enable)
        {
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var statusBar = (IVsStatusbar)Package.GetGlobalService(typeof(SVsStatusbar));

                return statusBar.Animation(enable ? 1 : 0, ref icon) == VSConstants.S_OK;
            });
        }
    }
}
