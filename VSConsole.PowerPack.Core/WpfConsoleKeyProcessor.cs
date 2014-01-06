using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Console.PowerPack.Core
{
    public class WpfConsoleKeyProcessor : OleCommandFilter
    {
        private static readonly char[] NEWLINE_CHARS = new char[2]
        {
            '\n',
            '\r'
        };

        private ICompletionSession _completionSession;

        static WpfConsoleKeyProcessor()
        {
        }

        public WpfConsoleKeyProcessor(WpfConsole wpfConsole)
            : base(wpfConsole.VsTextView)
        {
            WpfConsole = wpfConsole;
            WpfTextView = wpfConsole.WpfTextView;
            CommandExpansion = wpfConsole.Factory.GetCommandExpansion(wpfConsole);
        }

        private WpfConsole WpfConsole { get; set; }

        private IWpfTextView WpfTextView { get; set; }

        private ICommandExpansion CommandExpansion { get; set; }

        private bool IsCaretInReadOnlyRegion
        {
            get
            {
                if (WpfConsole.InputLineStart.HasValue)
                    return WpfTextView.TextBuffer.IsReadOnly(WpfTextView.Caret.Position.BufferPosition.Position);
                return true;
            }
        }

        private bool IsCaretOnInputLine
        {
            get
            {
                SnapshotPoint? inputLineStart = WpfConsole.InputLineStart;
                if (!inputLineStart.HasValue)
                    return false;
                SnapshotSpan includingLineBreak = inputLineStart.Value.GetContainingLine().ExtentIncludingLineBreak;
                SnapshotPoint caretPosition = CaretPosition;
                if (!includingLineBreak.Contains(caretPosition))
                    return includingLineBreak.End == caretPosition;
                return true;
            }
        }

        private bool IsCaretAtInputLineStart
        {
            get
            {
                SnapshotPoint? inputLineStart = WpfConsole.InputLineStart;
                SnapshotPoint bufferPosition = WpfTextView.Caret.Position.BufferPosition;
                if (inputLineStart.HasValue)
                    return inputLineStart.GetValueOrDefault() == bufferPosition;
                return false;
            }
        }

        private SnapshotPoint CaretPosition
        {
            get { return WpfTextView.Caret.Position.BufferPosition; }
        }

        private bool IsSelectionReadonly
        {
            get
            {
                if (WpfTextView.Selection.IsEmpty)
                    return false;
                ITextBuffer buffer = WpfTextView.TextBuffer;
                return WpfTextView.Selection.SelectedSpans.Any(span => buffer.IsReadOnly(span));
            }
        }

        private ICompletionBroker CompletionBroker
        {
            get { return WpfConsole.Factory.CompletionBroker; }
        }

        private bool IsCompletionSessionActive
        {
            get
            {
                if (_completionSession != null)
                    return !_completionSession.IsDismissed;
                return false;
            }
        }

        private void ExecuteCommand(VSConstants.VSStd2KCmdID idCommand, object args = null)
        {
            OldChain.Execute(idCommand, args);
        }

        protected override int InternalExec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn,
            IntPtr pvaOut)
        {
            int hr = -2147221248;
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                if ((int) nCmdID == 26)
                {
                    if (IsCaretInReadOnlyRegion || IsSelectionReadonly)
                        hr = 0;
                    else
                        PasteText(ref hr);
                }
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch ((VSConstants.VSStd2KCmdID) nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.CANCEL:
                        if (IsCompletionSessionActive)
                        {
                            _completionSession.Dismiss();
                            hr = 0;
                            break;
                        }
                        if (!IsCaretInReadOnlyRegion)
                        {
                            WpfTextView.TextBuffer.Delete(WpfConsole.AllInputExtent);
                            hr = 0;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.LEFT_EXT_COL:
                    case VSConstants.VSStd2KCmdID.WORDPREV_EXT_COL:
                    case VSConstants.VSStd2KCmdID.LEFT:
                    case VSConstants.VSStd2KCmdID.LEFT_EXT:
                    case VSConstants.VSStd2KCmdID.WORDPREV:
                    case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
                        if (IsCaretAtInputLineStart)
                        {
                            hr = 0;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.BOL_EXT_COL:
                    case VSConstants.VSStd2KCmdID.BOL:
                    case VSConstants.VSStd2KCmdID.BOL_EXT:
                        if (IsCaretOnInputLine)
                        {
                            VirtualSnapshotPoint virtualBufferPosition =
                                WpfTextView.Caret.Position.VirtualBufferPosition;
                            WpfTextView.Caret.MoveTo(WpfConsole.InputLineStart.Value);
                            WpfTextView.Caret.EnsureVisible();
                            if ((int) nCmdID == 19)
                                WpfTextView.Selection.Clear();
                            else if ((int) nCmdID != 19)
                                WpfTextView.Selection.Select(
                                    WpfTextView.Selection.IsEmpty
                                        ? virtualBufferPosition.TranslateTo(WpfTextView.TextSnapshot)
                                        : WpfTextView.Selection.AnchorPoint,
                                    WpfTextView.Caret.Position.VirtualBufferPosition);
                            hr = 0;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (IsCompletionSessionActive &&
                            IsCommitChar((char) (ushort) Marshal.GetObjectForNativeVariant(pvaIn)))
                        {
                            if (_completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                            {
                                _completionSession.Commit();
                                break;
                            }
                            _completionSession.Dismiss();
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (IsCompletionSessionActive)
                        {
                            if (_completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                                _completionSession.Commit();
                            else
                                _completionSession.Dismiss();
                        }
                        else if (IsCaretOnInputLine || !IsCaretInReadOnlyRegion)
                        {
                            ExecuteCommand(VSConstants.VSStd2KCmdID.END, (object) null);
                            ExecuteCommand(VSConstants.VSStd2KCmdID.RETURN, (object) null);
                            WpfConsole.EndInputLine(false);
                        }
                        hr = 0;
                        break;
                    case VSConstants.VSStd2KCmdID.TAB:
                        if (!IsCaretInReadOnlyRegion)
                        {
                            if (IsCompletionSessionActive)
                                _completionSession.Commit();
                            else
                                TriggerCompletion();
                        }
                        hr = 0;
                        break;
                    case VSConstants.VSStd2KCmdID.UP:
                        if (!IsCompletionSessionActive && !IsCaretInReadOnlyRegion)
                        {
                            WpfConsole.NavigateHistory(-1);
                            hr = 0;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.DOWN:
                        if (!IsCompletionSessionActive && !IsCaretInReadOnlyRegion)
                        {
                            WpfConsole.NavigateHistory(1);
                            hr = 0;
                        }
                        break;
                }
            }
            return hr;
        }

        private void PasteText(ref int hr)
        {
            string text = Clipboard.GetText();
            int startIndex = 0;
            int index;
            if (string.IsNullOrEmpty(text) || (index = text.IndexOfAny(NEWLINE_CHARS)) < 0)
                return;
            ITextBuffer textBuffer = WpfTextView.TextBuffer;
            while (startIndex < text.Length)
            {
                string str = index >= 0 ? text.Substring(startIndex, index - startIndex) : text.Substring(startIndex);
                if (startIndex == 0)
                {
                    if (!WpfTextView.Selection.IsEmpty)
                        textBuffer.Replace(WpfTextView.Selection.SelectedSpans[0], str);
                    else
                        textBuffer.Insert(WpfTextView.Caret.Position.BufferPosition.Position, str);
                    this.Execute(VSConstants.VSStd2KCmdID.RETURN, (object) null);
                }
                else
                    WpfConsole.Dispatcher.PostInputLine(new InputLine(str, index >= 0));
                if (index >= 0)
                {
                    startIndex = index + 1;
                    char ch;
                    if (startIndex < text.Length && (ch = text[startIndex]) != text[index] && (ch == 10 || ch == 13))
                        ++startIndex;
                    index = startIndex < text.Length ? text.IndexOfAny(NEWLINE_CHARS, startIndex) : -1;
                }
                else
                    break;
            }
            hr = 0;
        }

        private static bool IsCommitChar(char c)
        {
            if (!char.IsPunctuation(c) || c == 45 || c == 95)
                return char.IsWhiteSpace(c);
            return true;
        }

        private void TriggerCompletion()
        {
            if (CommandExpansion == null)
                return;
            if (IsCompletionSessionActive)
            {
                _completionSession.Dismiss();
                _completionSession = null;
            }
            string inputLineText = WpfConsole.InputLineText;
            int caretIndex = CaretPosition - WpfConsole.InputLineStart.Value;
            SimpleExpansion simpleExpansion = null;
            try
            {
                simpleExpansion = CommandExpansion.GetExpansions(inputLineText, caretIndex);
            }
            catch (Exception ex)
            {
            }
            if (simpleExpansion == null || simpleExpansion.Expansions == null)
                return;
            string[] expansions = simpleExpansion.Expansions;
            if (expansions.Length == 1)
            {
                ReplaceTabExpansion(simpleExpansion.Start, simpleExpansion.Length, expansions[0]);
            }
            else
            {
                if (expansions.Length <= 1)
                    return;
                _completionSession = CompletionBroker.CreateCompletionSession(WpfTextView,
                    WpfTextView.TextSnapshot.CreateTrackingPoint(CaretPosition.Position, PointTrackingMode.Positive),
                    true);
                _completionSession.Properties.AddProperty("TabExpansion", simpleExpansion);
                _completionSession.Dismissed += CompletionSession_Dismissed;
                _completionSession.Start();
            }
        }

        private void ReplaceTabExpansion(int lastWordIndex, int length, string expansion)
        {
            if (string.IsNullOrEmpty(expansion))
                return;
            WpfTextView.TextBuffer.Replace(WpfConsole.GetInputLineExtent(lastWordIndex, length), expansion);
        }

        private void CompletionSession_Dismissed(object sender, EventArgs e)
        {
            _completionSession.Dismissed -= CompletionSession_Dismissed;
            _completionSession = null;
        }
    }
}