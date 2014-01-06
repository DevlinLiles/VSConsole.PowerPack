using Microsoft.VisualStudio.Text.Editor;

namespace VSConsole.PowerPack.Core
{
    public interface ITextFormatClassifierProvider
    {
        ITextFormatClassifier GetTextFormatClassifier(ITextView textView);
    }
}