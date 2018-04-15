using System;
using System.Collections.Generic;

namespace System
{
    public struct ValueTuple<T1, T2, T3>
    {
        private static readonly EqualityComparer<T1> _t1Comparer = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> _t2Comparer = EqualityComparer<T2>.Default;
        private static readonly EqualityComparer<T3> _t3Comparer = EqualityComparer<T3>.Default;

        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3) { Item1 = item1; Item2 = item2; Item3 = item3; }

        public override int GetHashCode()
        {
            int hash = 701;
            unchecked
            {
                hash = hash * 269 + _t1Comparer.GetHashCode(Item1);
                hash = hash * 269 + _t2Comparer.GetHashCode(Item2);
                hash = hash * 269 + _t3Comparer.GetHashCode(Item3);
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            return obj is ValueTuple<T1, T2, T3> && Equals((ValueTuple<T1, T2, T3>)obj);
        }

        public bool Equals(ValueTuple<T1, T2, T3> obj)
        {
            return _t1Comparer.Equals(Item1, obj.Item1)
                && _t2Comparer.Equals(Item2, obj.Item2)
                && _t3Comparer.Equals(Item3, obj.Item3);
        }

        public override string ToString()
        {
            return "(" + Item1 + "," + Item2 + "," + Item3 + ")";
        }
    }
}
