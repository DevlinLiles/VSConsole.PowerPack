using Microsoft.VisualStudio.Text;

namespace VSConsole.PowerPack.Core
{
    public interface IGetSpan<T>
    {
        Span GetSpan(T t);
    }
}