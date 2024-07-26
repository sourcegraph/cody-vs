using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    internal class RunningDocTableHandlers : IVsRunningDocTableEvents
    {
        private readonly Action<uint> onAfterSave;
        private readonly Action<uint, IVsWindowFrame> onShow;
        private readonly Action<uint, IVsWindowFrame> onHide;

        public RunningDocTableHandlers(Action<uint> onAfterSave, Action<uint, IVsWindowFrame> onShow, Action<uint, IVsWindowFrame> onHide)
        {
            if (onAfterSave == null) throw new ArgumentNullException(nameof(onAfterSave));
            if (onShow == null) throw new ArgumentNullException(nameof(onShow));
            if (onHide == null) throw new ArgumentNullException(nameof(onHide));

            this.onAfterSave = onAfterSave;
            this.onShow = onShow;
            this.onHide = onHide;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterSave(uint docCookie)
        {
            onAfterSave(docCookie);
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            onShow(docCookie, pFrame);
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            onHide(docCookie, pFrame);
            return VSConstants.S_OK;
        }
    }
}
