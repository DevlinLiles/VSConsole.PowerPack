using System.Collections.Generic;

namespace VSConsole.PowerPack.Core
{
    public class InputHistory
    {
        private const int MAX_HISTORY = 50;
        private readonly Queue<string> _inputs = new Queue<string>();

        public IList<string> History
        {
            get { return _inputs.ToArray(); }
        }

        public void Add(string input)
        {
            if (string.IsNullOrEmpty(input))
                return;
            _inputs.Enqueue(input);
            if (_inputs.Count < 50)
                return;
            _inputs.Dequeue();
        }
    }
}