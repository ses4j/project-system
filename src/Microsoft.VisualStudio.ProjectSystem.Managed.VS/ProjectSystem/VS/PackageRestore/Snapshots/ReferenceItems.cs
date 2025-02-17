﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

using NuGet.SolutionRestoreManager;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    /// <summary>
    ///     Immutable collection of <see cref="IVsReferenceItem"/> objects.
    /// </summary>
    internal class ReferenceItems : ImmutablePropertyCollection<IVsReferenceItem>, IVsReferenceItems
    {
        public ReferenceItems(IEnumerable<IVsReferenceItem> items) 
            : base(items)
        {
        }

        protected override string GetKeyForItem(IVsReferenceItem value) => value.Name;
    }
}
