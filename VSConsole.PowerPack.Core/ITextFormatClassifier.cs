using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace Console.PowerPack.Core
{
    public interface ITextFormatClassifier
    {
        IClassificationType GetClassificationType(Color? foreground, Color? background);
    }
}