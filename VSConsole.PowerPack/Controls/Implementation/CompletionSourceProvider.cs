using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace VSConsole.PowerPack.Core
{
    [Name("Console.PowerPackCompletion")]
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("VsConsole")]
    public class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        public IWpfConsoleService WpfConsoleService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return WpfConsoleService.TryCreateCompletionSource(textBuffer) as ICompletionSource;
        }
    }
}