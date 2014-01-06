namespace VSConsole.PowerPack.Core
{
    public class SimpleExpansion
    {
        public SimpleExpansion(int start, int length, string[] expansions)
        {
            Start = start;
            Length = length;
            Expansions = expansions;
        }

        public int Start { get; private set; }

        public int Length { get; private set; }

        public string[] Expansions { get; private set; }
    }
}