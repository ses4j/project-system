﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem
{
    /// <summary>
    ///     Provides properties for retrieving options for the project system.
    /// </summary>
    [ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne)]
    internal interface IProjectSystemOptions
    {
        /// <summary>
        ///     Gets a value indicating if the project output pane is enabled.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if the project output pane is enabled; otherwise, <see langword="false"/>.
        /// </value>
        bool IsProjectOutputPaneEnabled
        {
            get;
        }

        /// <summary>
        ///     Gets a value indicating if the project fast up to date check is enabled.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token whose cancellation signals lost interest in the result.
        /// </param>
        /// <value>
        ///     <see langword="true"/> if the project fast up to date check is enabled; otherwise, <see langword="false"/>
        /// </value>
        Task<bool> GetIsFastUpToDateCheckEnabledAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a value indicating the level of fast up to date check logging.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token whose cancellation signals lost interest in the result.
        /// </param>
        /// <value>
        ///     The level of fast up to date check logging.
        /// </value>
        Task<LogLevel> GetFastUpToDateLoggingLevelAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a value indicating whether the designer view is the default editor for the specified designer category.
        /// </summary>
        Task<bool> GetUseDesignerByDefaultAsync(string designerCategory, bool defaultValue, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Sets a value indicating whether the designer view is the default editor for the specified designer category.
        /// </summary>
        Task SetUseDesignerByDefaultAsync(string designerCategory, bool value, CancellationToken cancellationToken = default);
    }
}
