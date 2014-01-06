using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace VSConsole.PowerPack.Core
{
    public class OrderedTupleSpans<T> : OrderedSpans<Tuple<Span, T>>
    {
        public OrderedTupleSpans()
            : base(new TupleGetSpan())
        {
        }

        public IEnumerable<Tuple<Span, T>> Overlap(Span span)
        {
            return base.Overlap(Tuple.Create(span, default(T)));
        }

        private class TupleGetSpan : IGetSpan<Tuple<Span, T>>
        {
            public Span GetSpan(Tuple<Span, T> t)
            {
                return t.Item1;
            }
        }
    }
}