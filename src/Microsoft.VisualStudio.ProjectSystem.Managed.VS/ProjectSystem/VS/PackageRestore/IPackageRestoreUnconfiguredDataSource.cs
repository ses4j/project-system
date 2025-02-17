﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Composition;
using NuGet.SolutionRestoreManager;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    /// <summary>
    ///     Represents the data source of metadata needed for restore operations for a <see cref="UnconfiguredProject"/>
    ///     instance by resolving conflicts and combining the data of all implicitly active <see cref="ConfiguredProject"/> 
    ///     instances.
    /// </summary>
    [ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne)]
    internal interface IPackageRestoreUnconfiguredDataSource : IProjectValueDataSource<IVsProjectRestoreInfo2>
    {
    }
}
