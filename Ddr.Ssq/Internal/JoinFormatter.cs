using System.Collections.Generic;

namespace Ddr.Ssq.Internal
{
    internal class JoinFormatter
    {
        readonly string Separator;
        readonly IEnumerable<string> Enumerable;
        public JoinFormatter(string Separator, IEnumerable<string> Enumerable)
            => (this.Separator, this.Enumerable) = (Separator, Enumerable);
        public override string ToString()
            => string.Join(Separator, Enumerable);

    }
}
