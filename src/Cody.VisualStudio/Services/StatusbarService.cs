using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cody.Core.Infrastructure;

namespace Cody.VisualStudio.Services
{
    public class StatusbarService : IStatusbarService
    {
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
    }
}
