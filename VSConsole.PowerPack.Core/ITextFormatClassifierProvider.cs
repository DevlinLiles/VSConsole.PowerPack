using Microsoft.VisualStudio.Text.Editor;

namespace Console.PowerPack.Core
{
    public interface ITextFormatClassifierProvider
    {
        ITextFormatClassifier GetTextFormatClassifier(ITextView textView);
    }
}