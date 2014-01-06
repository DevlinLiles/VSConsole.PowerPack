using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace VSConsole.PowerPack.Core
{
    [Export(typeof (IWpfConsoleService))]
    public class WpfConsoleService : IWpfConsoleService
    {
        private IClassificationType[] _tokenClassifications;

        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        internal IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService { get; set; }

        [Import]
        internal IEditorOptionsFactoryService EditorOptionsFactoryService { get; set; }

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal ITextFormatClassifierProvider TextFormatClassifierProvider { get; set; }

        [ImportMany(typeof (ICommandExpansionProvider))]
        internal List<Lazy<ICommandExpansionProvider, IHostNameMetadata>> CommandExpansionProviders { get; set; }

        [ImportMany(typeof (ICommandTokenizerProvider))]
        internal List<Lazy<ICommandTokenizerProvider, IHostNameMetadata>> CommandTokenizerProviders { get; set; }

        [Import]
        public IStandardClassificationService StandardClassificationService { get; set; }

        public IWpfConsole CreateConsole(IServiceProvider sp, string contentTypeName, string hostName)
        {
            return new WpfConsole(this, sp, contentTypeName, hostName).MarshalledConsole;
        }

        public object TryCreateCompletionSource(object textBuffer)
        {
            return new WpfConsoleCompletionSource(this, (ITextBuffer) textBuffer);
        }

        public object GetClassifier(object textBuffer)
        {
            var buffer = (ITextBuffer) textBuffer;
            return
                buffer.Properties.GetOrCreateSingletonProperty(
                    () => (IClassifier) new WpfConsoleClassifier(this, buffer));
        }

        private IService GetSingletonHostService<IService, IServiceFactory>(WpfConsole console,
            IEnumerable<Lazy<IServiceFactory, IHostNameMetadata>> providers,
            Func<IServiceFactory, IHost, IService> create, Func<IService> def) where IService : class
        {
            return console.WpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                IService local_0 = default(IService);
                Lazy<IServiceFactory, IHostNameMetadata> local_1 =
                    providers.FirstOrDefault(f => string.Equals(f.Metadata.HostName, console.HostName));
                if (local_1 != null)
                    local_0 = create(local_1.Value, console.Host);
                IService temp_25 = local_0;
                if (temp_25 != null)
                    return temp_25;
                return def();
            });
        }

        public ICommandExpansion GetCommandExpansion(WpfConsole console)
        {
            return GetSingletonHostService(console, CommandExpansionProviders, (factory, host) => factory.Create(host),
                () => (ICommandExpansion) null);
        }

        public ICommandTokenizer GetCommandTokenizer(WpfConsole console)
        {
            return GetSingletonHostService(console, CommandTokenizerProviders, (factory, host) => factory.Create(host),
                () => (ICommandTokenizer) null);
        }

        public IClassificationType GetTokenTypeClassification(TokenType tokenType)
        {
            if (_tokenClassifications == null)
                _tokenClassifications = new IClassificationType[16]
                {
                    StandardClassificationService.CharacterLiteral,
                    StandardClassificationService.Comment,
                    StandardClassificationService.ExcludedCode,
                    StandardClassificationService.FormalLanguage,
                    StandardClassificationService.Identifier,
                    StandardClassificationService.Keyword,
                    StandardClassificationService.Literal,
                    StandardClassificationService.NaturalLanguage,
                    StandardClassificationService.NumberLiteral,
                    StandardClassificationService.Operator,
                    StandardClassificationService.Other,
                    StandardClassificationService.PreprocessorKeyword,
                    StandardClassificationService.StringLiteral,
                    StandardClassificationService.SymbolDefinition,
                    StandardClassificationService.SymbolReference,
                    StandardClassificationService.WhiteSpace
                };
            var index = (int) tokenType;
            if (index < 0 || index >= _tokenClassifications.Length)
                index = 10;
            return _tokenClassifications[index];
        }
    }
}