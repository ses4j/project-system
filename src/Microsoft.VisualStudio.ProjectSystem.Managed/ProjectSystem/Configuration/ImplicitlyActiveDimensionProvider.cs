﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.Buffers.PooledObjects;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.Configuration
{
    /// <summary>
    ///     Provides an implementation of <see cref="IImplicitlyActiveDimensionProvider"/> that bases
    ///     itself on <see cref="IProjectConfigurationDimensionsProvider"/> instances.
    /// </summary>
    [Export(typeof(IImplicitlyActiveDimensionProvider))]
    internal class ImplicitlyActiveDimensionProvider : IImplicitlyActiveDimensionProvider
    {
        private readonly Lazy<ImmutableArray<string>> _builtInImplicitlyActiveDimensions;

        [ImportingConstructor]
        public ImplicitlyActiveDimensionProvider(UnconfiguredProject project)
        {
            _builtInImplicitlyActiveDimensions = new Lazy<ImmutableArray<string>>(CalculateBuiltInImplicitlyActiveDimensions);

            DimensionProviders = new OrderPrecedenceImportCollection<IProjectConfigurationDimensionsProvider, IConfigurationDimensionDescriptionMetadataView>(projectCapabilityCheckProvider: project);
        }

        [ImportMany]
        internal OrderPrecedenceImportCollection<IProjectConfigurationDimensionsProvider, IConfigurationDimensionDescriptionMetadataView> DimensionProviders
        {
            get;
        }

        public IEnumerable<string> GetImplicitlyActiveDimensions(IEnumerable<string> dimensionNames)
        {
            Requires.NotNull(dimensionNames, nameof(dimensionNames));

            ImmutableArray<string> builtInDimensions = _builtInImplicitlyActiveDimensions.Value;

            // NOTE: Order matters; this must be in the order in which the providers are
            // prioritized in 'builtInDimensions', of which Enumerable.Intersect guarantees.
            return builtInDimensions.Intersect(dimensionNames, StringComparers.ConfigurationDimensionNames);
        }

        private ImmutableArray<string> CalculateBuiltInImplicitlyActiveDimensions()
        {
            var implicitlyActiveDimensions = PooledArray<string>.GetInstance();

            foreach (Lazy<IProjectConfigurationDimensionsProvider, IConfigurationDimensionDescriptionMetadataView> provider in DimensionProviders)
            {
                for (int i = 0; i < provider.Metadata.IsVariantDimension.Length; i++)
                {
                    if (provider.Metadata.IsVariantDimension[i])
                    {
                        if (!implicitlyActiveDimensions.Contains(provider.Metadata.DimensionName[i], StringComparers.ConfigurationDimensionNames))
                            implicitlyActiveDimensions.Add(provider.Metadata.DimensionName[i]);
                    }
                }
            }

            return implicitlyActiveDimensions.ToImmutableAndFree();
        }
    }
}
