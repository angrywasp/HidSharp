using System.Collections.Generic;
using System.Linq;

namespace HidSharp.Reports
{
    public class Indexes
    {
        static readonly Indexes _unset = new Indexes();

        public bool ContainsValue(uint value)
        {
            return TryGetIndexFromValue(value, out int index);
        }

        public IEnumerable<uint> GetAllValues()
        {
            return Enumerable.Range(0, Count).SelectMany(index => GetValuesFromIndex(index));
        }

        public virtual bool TryGetIndexFromValue(uint value, out int elementIndex)
        {
            elementIndex = -1; return false;
        }

        public virtual IEnumerable<uint> GetValuesFromIndex(int elementIndex)
        {
            yield break;
        }

        public virtual int Count
        {
            get { return 0; }
        }

        public static Indexes Unset
        {
            get { return _unset; }
        }
    }
}
