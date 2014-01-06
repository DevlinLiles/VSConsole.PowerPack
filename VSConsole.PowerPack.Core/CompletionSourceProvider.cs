using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Console.PowerPack.Core
{
    [Name("Console.PowerPackCompletion")]
    [Export(typeof (ICompletionSourceProvider))]
    [ContentType("Console.PowerPack")]
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