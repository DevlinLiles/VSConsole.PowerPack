using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Console.PowerPack.Core
{
    public class TextFormatClassifier : ObjectWithFactory<TextFormatClassifierProvider>, ITextFormatClassifier
    {
        private readonly Dictionary<Tuple<Color?, Color?>, IClassificationType> _classificationMap =
            new Dictionary<Tuple<Color?, Color?>, IClassificationType>();

        private readonly ITextView _textView;

        public TextFormatClassifier(TextFormatClassifierProvider factory, ITextView textView)
            : base(factory)
        {
            UtilityMethods.ThrowIfArgumentNull(textView);
            _textView = textView;
        }

        public IClassificationType GetClassificationType(Color? foreground, Color? background)
        {
            Tuple<Color?, Color?> key = Tuple.Create(foreground, background);
            IClassificationType classificationType;
            if (!_classificationMap.TryGetValue(key, out classificationType))
            {
                string classificationName = GetClassificationName(foreground, background);
                classificationType = Factory.ClassificationTypeRegistryService.GetClassificationType(classificationName);
                if (classificationType == null)
                    classificationType =
                        Factory.ClassificationTypeRegistryService.CreateClassificationType(classificationName,
                            new IClassificationType[1]
                            {
                                Factory.StandardClassificationService.NaturalLanguage
                            });
                _classificationMap.Add(key, classificationType);
                Factory.ClassificationFormatMapService.GetClassificationFormatMap(_textView)
                    .SetTextProperties(classificationType, GetFormat(foreground, background));
            }
            return classificationType;
        }

        private static string GetClassificationName(Color? foreground, Color? background)
        {
            var stringBuilder = new StringBuilder(32);
            if (foreground.HasValue)
                stringBuilder.Append(foreground.Value);
            stringBuilder.Append('-');
            if (background.HasValue)
                stringBuilder.Append(background.Value);
            return stringBuilder.ToString();
        }

        private static TextFormattingRunProperties GetFormat(Color? foreground, Color? background)
        {
            TextFormattingRunProperties formattingRunProperties =
                TextFormattingRunProperties.CreateTextFormattingRunProperties();
            if (foreground.HasValue)
                formattingRunProperties = formattingRunProperties.SetForeground(foreground.Value);
            if (background.HasValue)
                formattingRunProperties = formattingRunProperties.SetBackground(background.Value);
            return formattingRunProperties;
        }
    }
}