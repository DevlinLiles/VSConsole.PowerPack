using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public class OrderedSpans<T>
    {
        private readonly IGetSpan<T> _getSpan;
        private readonly List<T> _items = new List<T>();

        public OrderedSpans(IGetSpan<T> getSpan)
        {
            UtilityMethods.ThrowIfArgumentNull(getSpan);
            _getSpan = getSpan;
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public T this[int i]
        {
            get { return _items[i]; }
        }

        private Span GetSpan(T t)
        {
            return _getSpan.GetSpan(t);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void Add(T t)
        {
            if (_items.Count > 0 && GetSpan(t).Start < GetSpan(_items[_items.Count - 1]).End)
                throw new InvalidOperationException();
            _items.Add(t);
        }

        public void PopLast()
        {
            _items.RemoveAt(_items.Count - 1);
        }

        public int FindFirstOverlap(T t)
        {
            if (_items.Count > 0)
            {
                Span span1 = GetSpan(t);
                int index1 = _items.Count - 1;
                Span span2 = GetSpan(_items[index1]);
                if (span2.Start <= span1.Start)
                {
                    if (!span2.OverlapsWith(span1))
                        return -1;
                    return index1;
                }
                int index2 = _items.BinarySearch(t, new SpanStartComparer(_getSpan));
                if (index2 < 0)
                    index2 = Math.Max(0, ~index2 - 1);
                for (; index2 < _items.Count; ++index2)
                {
                    Span span3 = GetSpan(_items[index2]);
                    if (span3.OverlapsWith(span1))
                        return index2;
                    if (span3.Start >= span1.End)
                        return -1;
                }
            }
            return -1;
        }

        public IEnumerable<T> Overlap(T t)
        {
            int index = FindFirstOverlap(t);
            if (index >= 0)
            {
                for (Span span = GetSpan(t); index < _items.Count && GetSpan(_items[index]).OverlapsWith(span); ++index)
                    yield return _items[index];
            }
        }

        private class SpanStartComparer : Comparer<T>
        {
            private readonly IGetSpan<T> _getSpan;

            public SpanStartComparer(IGetSpan<T> getSpan)
            {
                _getSpan = getSpan;
            }

            public override int Compare(T x, T y)
            {
                return _getSpan.GetSpan(x).Start.CompareTo(_getSpan.GetSpan(y).Start);
            }
        }
    }
}