﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeSheetCore.Helpers
{
    public static class LinqExtensions
    {
        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }
    }
}
