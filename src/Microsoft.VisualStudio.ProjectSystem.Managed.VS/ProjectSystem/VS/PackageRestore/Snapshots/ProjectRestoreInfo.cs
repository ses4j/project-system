﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using NuGet.SolutionRestoreManager;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    /// <summary>
    ///     Concrete implementation of <see cref="IVsProjectRestoreInfo2"/> that will be passed to 
    ///     <see cref="IVsSolutionRestoreService3.NominateProjectAsync(string, IVsProjectRestoreInfo2, System.Threading.CancellationToken)"/>.
    /// </summary>
    internal class ProjectRestoreInfo : IVsProjectRestoreInfo2
    {
        public ProjectRestoreInfo(string msbuildProjectExtensionsPath, string originalTargetFrameworks, IVsTargetFrameworks2 targetFrameworks, IVsReferenceItems toolReferences)
        {
            Requires.NotNull(msbuildProjectExtensionsPath, nameof(msbuildProjectExtensionsPath));
            Requires.NotNull(originalTargetFrameworks, nameof(originalTargetFrameworks));
            Requires.NotNull(targetFrameworks, nameof(targetFrameworks));
            Requires.NotNull(toolReferences, nameof(toolReferences));

            MSBuildProjectExtensionsPath = msbuildProjectExtensionsPath;
            OriginalTargetFrameworks = originalTargetFrameworks;
            TargetFrameworks = targetFrameworks;
            ToolReferences = toolReferences;
        }

        public string MSBuildProjectExtensionsPath { get; }

        public string OriginalTargetFrameworks { get; }

        public IVsTargetFrameworks2 TargetFrameworks { get; }

        public IVsReferenceItems ToolReferences { get; }

        // We "rename" BaseIntermediatePath to avoid confusion for our usage, 
        // because it actually represents "MSBuildProjectExtensionsPath"
        string IVsProjectRestoreInfo2.BaseIntermediatePath
        {
            get { return MSBuildProjectExtensionsPath; }
        }
    }
}
