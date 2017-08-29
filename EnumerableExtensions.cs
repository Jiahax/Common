﻿namespace Microsoft.OpenPublishing.Build.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => !enumerable?.Any() ?? true;

        public static T PickRandom<T>(this IEnumerable<T> enumerable)
        {
            Guard.Argument(() => !enumerable.IsNullOrEmpty(), nameof(enumerable));

            var list = enumerable.ToList();
            return list[StaticRandom.Rand() % list.Count];
        }
    }
}