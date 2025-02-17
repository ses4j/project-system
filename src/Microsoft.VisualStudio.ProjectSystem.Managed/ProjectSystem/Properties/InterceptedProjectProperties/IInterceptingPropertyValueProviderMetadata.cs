﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.Properties
{
    /// <summary>
    /// Metadata mapping interface for the <see cref="ExportInterceptingPropertyValueProviderAttribute"/>.
    /// </summary>
    public interface IInterceptingPropertyValueProviderMetadata
    {
        string PropertyName { get; }
    }
}