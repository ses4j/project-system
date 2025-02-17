﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable disable

namespace System.Collections.Immutable
{
    internal static class ImmutableStringHashSet
    {
        public static readonly ImmutableHashSet<string> EmptyOrdinal
            = ImmutableHashSet<string>.Empty;

        public static readonly ImmutableHashSet<string> EmptyOrdinalIgnoreCase
            = ImmutableHashSet<string>.Empty.WithComparer(StringComparer.OrdinalIgnoreCase);
    }
}
