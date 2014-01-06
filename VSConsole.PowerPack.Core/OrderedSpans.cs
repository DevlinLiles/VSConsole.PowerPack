using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public class OrderedSpans : OrderedSpans<Span>
    {
        public OrderedSpans()
            : base(new SpanGetSpan())
        {
        }

        private class SpanGetSpan : IGetSpan<Span>
        {
            public Span GetSpan(Span t)
            {
                return t;
            }
        }
    }
}