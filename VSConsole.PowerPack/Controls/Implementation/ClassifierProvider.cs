using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VSConsole.PowerPack.Core
{
    [Export(typeof (IClassifierProvider))]
    [ContentType("VsConsole")]
    public class ClassifierProvider : IClassifierProvider
    {
        [Import]
        public IWpfConsoleService WpfConsoleService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return WpfConsoleService.GetClassifier(textBuffer) as IClassifier;
        }
    }
}