﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;

using Microsoft.VisualStudio.ProjectSystem.Imaging;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem
{
    /// <summary>
    ///     Provides the base class for <see cref="IProjectTreePropertiesProvider"/> objects that handle special items, such as the AppDesigner folder.
    /// </summary>
    internal abstract class AbstractSpecialFolderProjectTreePropertiesProvider : IProjectTreePropertiesProvider
    {
        private readonly IProjectImageProvider _imageProvider;

        protected AbstractSpecialFolderProjectTreePropertiesProvider(IProjectImageProvider imageProvider)
        {
            Assumes.NotNull(imageProvider);

            _imageProvider = imageProvider;
        }

        /// <summary>
        ///     Gets the image key that represents the image that will be applied to the special folder.
        /// </summary>
        public abstract string FolderImageKey
        {
            get;
        }

        /// <summary>
        ///     Gets the default flags that will be applied to the special folder.
        /// </summary>
        public abstract ProjectTreeFlags FolderFlags
        {
            get;
        }

        /// <summary>
        ///     Gets a value indicating whether the special folder is supported in this project.
        /// </summary>
        public abstract bool IsSupported
        {
            get;
        }

        public void CalculatePropertyValues(IProjectTreeCustomizablePropertyContext propertyContext, IProjectTreeCustomizablePropertyValues propertyValues)
        {
            Requires.NotNull(propertyContext, nameof(propertyContext));
            Requires.NotNull(propertyValues, nameof(propertyValues));

            if (!IsSupported)
                return;

            if (IsCandidateSpecialFolder(propertyContext, propertyValues.Flags))
            {
                ApplySpecialFolderProperties(propertyValues);
                return;
            }

            if (AreContentsVisibleOnlyInShowAllFiles(propertyContext.ProjectTreeSettings) && IsCandidateSpecialFolderItem(propertyContext, propertyValues.Flags))
            {
                ApplySpecialFolderItemProperties(propertyValues);
                return;
            }
        }

        /// <summary>
        ///     Returns a value indicating whether the specified property context represents the candidate special folder.
        /// </summary>
        protected abstract bool IsCandidateSpecialFolder(IProjectTreeCustomizablePropertyContext propertyContext, ProjectTreeFlags flags);

        /// <summary>
        ///     Returns a value indicating whether the contents of the special folder are only visible in Show All Files.
        /// </summary>
        protected abstract bool AreContentsVisibleOnlyInShowAllFiles(IImmutableDictionary<string, string> projectTreeSettings);

        private bool IsCandidateSpecialFolderItem(IProjectTreeCustomizablePropertyContext propertyContext, ProjectTreeFlags flags)
        {
            // We're a special folder item if our parent is the special folder. We rely on 
            // the fact that "VisibleOnlyInShowAllFiles" is transitive; that is, if a parent
            // is marked with it, its children are also implicitly marked with it.
            if (propertyContext.ParentNodeFlags.Contains(FolderFlags))
            {
                return flags.IsIncludedInProject();
            }

            return false;
        }

        private void ApplySpecialFolderProperties(IProjectTreeCustomizablePropertyValues propertyValues)
        {
            propertyValues.Flags = propertyValues.Flags.Union(FolderFlags);

            // Avoid overwriting icon if the image provider didn't provide one
            ProjectImageMoniker icon = _imageProvider.GetProjectImage(FolderImageKey);
            if (icon != null)
            {
                propertyValues.Icon = icon;
                propertyValues.ExpandedIcon = icon;
            }
        }

        private static void ApplySpecialFolderItemProperties(IProjectTreeCustomizablePropertyValues propertyValues)
        {
            propertyValues.Flags = propertyValues.Flags.Add(ProjectTreeFlags.Common.VisibleOnlyInShowAllFiles);
        }
    }
}
