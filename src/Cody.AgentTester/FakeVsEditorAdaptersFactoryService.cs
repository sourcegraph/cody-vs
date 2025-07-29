using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    public class FakeVsEditorAdaptersFactoryService : IVsEditorAdaptersFactoryService
    {
        public IVsCodeWindow CreateVsCodeWindowAdapter(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider, IContentType contentType)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapterForSecondaryBuffer(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider, ITextBuffer secondaryBuffer)
        {
            throw new NotImplementedException();
        }

        public IVsTextBufferCoordinator CreateVsTextBufferCoordinatorAdapter()
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider, ITextViewRoleSet roles)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer GetBufferAdapter(ITextBuffer textBuffer)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDataBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDocumentBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public IVsTextView GetViewAdapter(ITextView textView)
        {
            throw new NotImplementedException();
        }

        public IWpfTextView GetWpfTextView(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public IWpfTextViewHost GetWpfTextViewHost(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public void SetDataBuffer(IVsTextBuffer bufferAdapter, ITextBuffer dataBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
