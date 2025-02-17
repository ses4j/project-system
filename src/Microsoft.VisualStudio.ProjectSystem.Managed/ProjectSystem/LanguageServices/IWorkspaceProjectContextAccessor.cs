﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.LanguageServices.ProjectSystem;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.LanguageServices
{
    /// <summary>
    ///     Provides access to a <see cref="IWorkspaceProjectContext"/> and associated services.
    /// </summary>
    internal interface IWorkspaceProjectContextAccessor
    {
        /// <summary>
        ///     Gets an identifier that uniquely identifies the <see cref="IWorkspaceProjectContext"/> across a solution.
        /// </summary>
        string ContextId
        {
            get;
        }

        /// <summary>
        ///     Gets the <see cref="IWorkspaceProjectContext"/> that provides access to the language service.
        /// </summary>
        IWorkspaceProjectContext Context
        {
            get;
        }

        /// <summary>
        ///     Gets the object that represents the host specific "Edit and Continue" service.
        /// </summary>
        /// <remarks>
        ///     Within a Visual Studio host, this is typically an object implementing IVsENCRebuildableProjectCfg2 and IVsENCRebuildableProjectCfg4.
        /// </remarks>
        object HostSpecificEditAndContinueService
        {
            get;
        }

        /// <summary>
        ///     Gets an object that represents a host-specific error reporter.
        /// </summary>
        /// <remarks>
        ///     Within a Visual Studio host, this is typically an object implementing IVsLanguageServiceBuildErrorReporter2.
        /// </remarks>
        object HostSpecificErrorReporter
        {
            get;
        }
    }
}
