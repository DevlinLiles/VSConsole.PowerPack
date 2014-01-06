using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Console.PowerPack.Core
{
    public class OleCommandFilter : IOleCommandTarget
    {
        public const int OLECMDERR_E_NOTSUPPORTED = -2147221248;

        public OleCommandFilter(IVsTextView vsTextView)
        {
            IOleCommandTarget ppNextCmdTarg;
            ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out ppNextCmdTarg));
            OldChain = ppNextCmdTarg;
        }

        protected IOleCommandTarget OldChain { get; private set; }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            int num = InternalQueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            if (num == -2147221248)
                num = OldChain.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            return num;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int num = InternalExec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            if (num == -2147221248)
                num = OldChain.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            return num;
        }

        protected virtual int InternalQueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return -2147221248;
        }

        protected virtual int InternalExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn,
            IntPtr pvaOut)
        {
            return -2147221248;
        }
    }
}