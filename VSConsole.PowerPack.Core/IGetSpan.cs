using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public interface IGetSpan<T>
    {
        Span GetSpan(T t);
    }
}