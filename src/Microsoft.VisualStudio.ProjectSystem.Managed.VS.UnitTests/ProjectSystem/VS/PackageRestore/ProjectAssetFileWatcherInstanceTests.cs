﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Moq;

using Xunit;

using Task = System.Threading.Tasks.Task;

using static Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore.ProjectAssetFileWatcher;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    public class ProjectAssetFileWatcherInstanceTests
    {
        private const string ProjectCurrentStateJson = @"{
    ""CurrentState"": {
        ""ConfigurationGeneral"": {
            ""Properties"": {
               ""BaseIntermediateOutputPath"": ""obj\\"",
               ""MSBuildProjectFullPath"": ""C:\\Foo\\foo.proj""
            }
        }
    }
}";

        [Theory]
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""", @"C:\Foo\obj\project.assets.json")]
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\project.json""", @"C:\Foo\project.lock.json")]
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    foo.project.json, FilePath: ""C:\Foo\foo.project.json""", @"C:\Foo\foo.project.lock.json")]
        public async Task VerifyFileWatcherRegistration(string inputTree, string fileToWatch)
        {
            uint adviseCookie = 100;
            var fileChangeService = IVsFileChangeExFactory.CreateWithAdviseUnadviseFileChange(adviseCookie);
            
            var tasksService = IUnconfiguredProjectTasksServiceFactory.ImplementLoadedProjectAsync<ConfiguredProject>(t => t());

            using var watcher = new ProjectAssetFileWatcherInstance(IVsServiceFactory.Create<SVsFileChangeEx, Shell.IVsAsyncFileChangeEx>(fileChangeService), IProjectTreeProviderFactory.Create(), IUnconfiguredProjectCommonServicesFactory.Create(threadingService: IProjectThreadingServiceFactory.Create()), tasksService, IActiveConfiguredProjectSubscriptionServiceFactory.Create());
            var tree = ProjectTreeParser.Parse(inputTree);
            var projectUpdate = IProjectSubscriptionUpdateFactory.FromJson(ProjectCurrentStateJson);
            await watcher.InitializeAsync();
            await watcher.DataFlow_ChangedAsync(IProjectVersionedValueFactory.Create(Tuple.Create(IProjectTreeSnapshotFactory.Create(tree), projectUpdate)));

            // If fileToWatch is null then we expect to not register any filewatcher.
            var times = fileToWatch == null ? Times.Never() : Times.Once();
            Mock.Get(fileChangeService).Verify(s => s.AdviseFileChangeAsync(fileToWatch ?? It.IsAny<string>(), It.IsAny<_VSFILECHANGEFLAGS>(), watcher, CancellationToken.None), times);
        }

        [Theory]
        // Add file
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""", @"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\project.json""", 2, 1)]
        // Remove file
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\project.json""", @"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""", 2, 1)]
        // Rename file to projectName.project.json
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\project.json""", @"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    foo.project.json, FilePath: ""C:\Foo\foo.project.json""", 2, 1)]
        // Rename file to somethingelse
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\foo.project.json""", @"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    fooproject.json, FilePath: ""C:\Foo\fooproject.json""", 2, 1)]
        // Unrelated change
        [InlineData(@"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\foo.project.json""", @"
Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""
    project.json, FilePath: ""C:\Foo\project.json""
    somefile.json, FilePath: ""C:\Foo\somefile.json""", 1, 0)]

        public async Task VerifyFileWatcherRegistrationOnTreeChange(string inputTree, string changedTree, int numRegisterCalls, int numUnregisterCalls)
        {
            uint adviseCookie = 100;
            var fileChangeService = IVsFileChangeExFactory.CreateWithAdviseUnadviseFileChange(adviseCookie);
            
            var tasksService = IUnconfiguredProjectTasksServiceFactory.ImplementLoadedProjectAsync<ConfiguredProject>(t => t());

            using var watcher = new ProjectAssetFileWatcherInstance(
                IVsServiceFactory.Create<SVsFileChangeEx, IVsAsyncFileChangeEx>(fileChangeService),
                IProjectTreeProviderFactory.Create(),
                IUnconfiguredProjectCommonServicesFactory.Create(threadingService: IProjectThreadingServiceFactory.Create()),
                tasksService,
                IActiveConfiguredProjectSubscriptionServiceFactory.Create());
            await watcher.InitializeAsync();
            var projectUpdate = IProjectSubscriptionUpdateFactory.FromJson(ProjectCurrentStateJson);

            var firstTree = ProjectTreeParser.Parse(inputTree);
            await watcher.DataFlow_ChangedAsync(IProjectVersionedValueFactory.Create(Tuple.Create(IProjectTreeSnapshotFactory.Create(firstTree), projectUpdate)));

            var secondTree = ProjectTreeParser.Parse(changedTree);
            await watcher.DataFlow_ChangedAsync(IProjectVersionedValueFactory.Create(Tuple.Create(IProjectTreeSnapshotFactory.Create(secondTree), projectUpdate)));

            // If fileToWatch is null then we expect to not register any filewatcher.
            var fileChangeServiceMock = Mock.Get(fileChangeService);
            fileChangeServiceMock.Verify(s => s.AdviseFileChangeAsync(It.IsAny<string>(), It.IsAny<_VSFILECHANGEFLAGS>(), watcher, CancellationToken.None),
                Times.Exactly(numRegisterCalls));
            fileChangeServiceMock.Verify(s => s.UnadviseFileChangeAsync(adviseCookie, CancellationToken.None), Times.Exactly(numUnregisterCalls));

        }

        [Fact]
        public async Task WhenBaseIntermediateOutputPathNotSet_DoesNotAttemptToAdviseFileChange()
        {
            var fileChangeService = IVsFileChangeExFactory.CreateWithAdviseUnadviseFileChange(100);
            
            var tasksService = IUnconfiguredProjectTasksServiceFactory.ImplementLoadedProjectAsync<ConfiguredProject>(t => t());

            using var watcher = new ProjectAssetFileWatcherInstance(
                IVsServiceFactory.Create<SVsFileChangeEx, IVsAsyncFileChangeEx>(fileChangeService),
                IProjectTreeProviderFactory.Create(),
                IUnconfiguredProjectCommonServicesFactory.Create(threadingService: IProjectThreadingServiceFactory.Create()),
                tasksService,
                IActiveConfiguredProjectSubscriptionServiceFactory.Create());
            var tree = ProjectTreeParser.Parse(@"Root (flags: {ProjectRoot}), FilePath: ""C:\Foo\foo.proj""");
            var projectUpdate = IProjectSubscriptionUpdateFactory.FromJson(@"{
    ""CurrentState"": {
        ""ConfigurationGeneral"": {
            ""Properties"": {
               ""MSBuildProjectFullPath"": ""C:\\Foo\\foo.proj""
            }
        }
    }
}");

            await watcher.InitializeAsync();
            await watcher.DataFlow_ChangedAsync(IProjectVersionedValueFactory.Create(Tuple.Create(IProjectTreeSnapshotFactory.Create(tree), projectUpdate)));

            var fileChangeServiceMock = Mock.Get(fileChangeService);
            fileChangeServiceMock.Verify(s => s.AdviseFileChangeAsync(It.IsAny<string>(), It.IsAny<_VSFILECHANGEFLAGS>(), watcher, CancellationToken.None),
                Times.Never());
        }
    }
}
