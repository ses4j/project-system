﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem.Imaging;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS.Imaging
{
    /// <summary>
    ///     Provides the AppDesigner folder image.
    /// </summary>
    [Export(typeof(IProjectImageProvider))]
    [AppliesTo(ProjectCapability.AppDesigner)]
    internal class AppDesignerFolderProjectImageProvider : IProjectImageProvider
    {
        [ImportingConstructor]
        public AppDesignerFolderProjectImageProvider()
        {
        }

        public ProjectImageMoniker GetProjectImage(string key)
        {
            Requires.NotNullOrEmpty(key, nameof(key));

            switch (key)
            {
                case ProjectImageKey.AppDesignerFolder:
                    return KnownMonikers.Property.ToProjectSystemType();

                default:
                    return null;
            }
        }
    }
}
