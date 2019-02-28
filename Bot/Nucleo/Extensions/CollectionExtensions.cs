﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Nucleo.Extensions
{
    public static class CollectionExtensions
    {
        public static ulong GetFirst(this IReadOnlyCollection<ulong> collection)
        {
            return collection.ElementAt(0);
        }
    }
}
//ok
