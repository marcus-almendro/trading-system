using System.Collections.Generic;
using System.Linq;

namespace TradingSystem.Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetAllValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType() || !(obj is ValueObject other))
                return false;

            return GetAllValues().Zip(other.GetAllValues(), (a, b) =>
            {
                if (a == null)
                    return b == null;

                return a.Equals(b);
            }).All(c => c);
        }

        public override int GetHashCode()
        {
            return GetAllValues()
             .Select(x => x != null ? x.GetHashCode() : 0)
             .Aggregate((x, y) => x ^ y);
        }
    }
}
