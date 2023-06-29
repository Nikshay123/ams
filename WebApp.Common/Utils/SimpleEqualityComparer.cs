using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WebApp.Common.Utils
{
    public class SimpleEqualityComparer<T> : EqualityComparer<T>
    {
        public SimpleEqualityComparer(Func<T, T, bool> cmp)
        {
            this.cmp = cmp;
        }

        public override bool Equals(T? x, T? y)
        {
            return cmp(x, y);
        }

        public override int GetHashCode([DisallowNull] T obj)
        {
            return obj.GetHashCode();
        }

        public Func<T, T, bool> cmp { get; set; }
    }
}