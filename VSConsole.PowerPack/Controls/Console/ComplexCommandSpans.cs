using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace VSConsole.PowerPack.Core
{
    public class ComplexCommandSpans : OrderedTupleSpans<bool>
    {
        public Span? CurrentCommandStart
        {
            get
            {
                if (Count > 0 && !this[Count - 1].Item2)
                {
                    int commandStart = FindCommandStart(Count - 1);
                    if (commandStart >= 0)
                        return this[commandStart].Item1;
                }
                return new Span?();
            }
        }

        public void Add(Span lineSpan, bool endCommand)
        {
            base.Add(Tuple.Create(lineSpan, endCommand));
        }

        public int FindCommandStart(int i)
        {
            while (i - 1 >= 0 && !this[i - 1].Item2)
                --i;
            return i;
        }

        public IEnumerable<IList<Span>> Overlap(Span span)
        {
            int i = FindFirstOverlap(Tuple.Create(span, true));
            if (i >= 0)
            {
                i = FindCommandStart(i);
                do
                {
                    var spans = new List<Span>();
                    while (i < Count)
                    {
                        spans.Add(this[i].Item1);
                        if (this[i++].Item2)
                            break;
                    }
                    yield return spans;
                } while (i < Count && this[i].Item1.OverlapsWith(span));
            }
        }
    }
}