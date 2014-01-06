using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace VSConsole.PowerPack.Core
{
    public interface ITextFormatClassifier
    {
        IClassificationType GetClassificationType(Color? foreground, Color? background);
    }
}