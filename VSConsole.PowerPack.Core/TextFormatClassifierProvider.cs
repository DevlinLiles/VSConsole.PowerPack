using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace Console.PowerPack.Core
{
    [Export(typeof (ITextFormatClassifierProvider))]
    public class TextFormatClassifierProvider : ITextFormatClassifierProvider
    {
        [Import]
        internal IStandardClassificationService StandardClassificationService { get; set; }

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        [Import]
        internal IClassificationFormatMapService ClassificationFormatMapService { get; set; }

        public ITextFormatClassifier GetTextFormatClassifier(ITextView textView)
        {
            UtilityMethods.ThrowIfArgumentNull(textView);
            return textView.Properties.GetOrCreateSingletonProperty(() => new TextFormatClassifier(this, textView));
        }
    }
}