﻿// Copyright(c) Microsoft.All Rights Reserved.Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem
{
    [Export]
    [Export(typeof(IUnconfiguredProjectTasksService))]
    [AppliesTo(ProjectCapability.DotNet)]
    internal class UnconfiguredProjectTasksService : IUnconfiguredProjectTasksService
    {
        private readonly IProjectAsynchronousTasksService _tasksService;
        private readonly IProjectThreadingService _threadingService;
        private readonly ILoadedInHostListener _loadedInHostListener;
        private readonly TaskCompletionSource<object> _projectLoadedInHost = new TaskCompletionSource<object>();
        private readonly TaskCompletionSource<object> _prioritizedProjectLoadedInHost = new TaskCompletionSource<object>();
        private readonly JoinableTaskCollection _prioritizedTasks;

        [ImportingConstructor]
        public UnconfiguredProjectTasksService(
            [Import(ExportContractNames.Scopes.UnconfiguredProject)]IProjectAsynchronousTasksService tasksService,
            IProjectThreadingService threadingService,
            [Import(AllowDefault = true)] ILoadedInHostListener loadedInHostListener)
        {
            _prioritizedTasks = threadingService.JoinableTaskContext.CreateCollection();
            _prioritizedTasks.DisplayName = "PrioritizedProjectLoadedInHostTasks";
            _tasksService = tasksService;
            _threadingService = threadingService;
            _loadedInHostListener = loadedInHostListener;
        }

#pragma warning disable RS0030 // symbol ProjectAutoLoad is banned
        [ProjectAutoLoad(completeBy: ProjectLoadCheckpoint.ProjectFactoryCompleted)]
#pragma warning restore RS0030 // symbol ProjectAutoLoad is banned
        [AppliesTo(ProjectCapability.DotNet)]
        public Task OnProjectFactoryCompleted()
        {
            if (_loadedInHostListener != null)
            {
                return _loadedInHostListener.StartListeningAsync();
            }
            else
            {
                _projectLoadedInHost.TrySetResult(false);
                return Task.CompletedTask;
            }
        }

        public Task ProjectLoadedInHost
        {
            get { return _projectLoadedInHost.Task.WithCancellation(_tasksService.UnloadCancellationToken); }
        }

        public Task PrioritizedProjectLoadedInHost
        {
            get { return _prioritizedProjectLoadedInHost.Task.WithCancellation(_tasksService.UnloadCancellationToken); }
        }

        public CancellationToken UnloadCancellationToken
        {
            get { return _tasksService.UnloadCancellationToken; }
        }

#pragma warning disable RS0030 // Do not use LoadedProjectAsync (this is the replacement)
        public Task LoadedProjectAsync(Func<Task> action)
        {
            JoinableTask joinable = _tasksService.LoadedProjectAsync(action);

            return joinable.Task;
        }

        public Task<T> LoadedProjectAsync<T>(Func<Task<T>> action)
        {
            JoinableTask<T> joinable = _tasksService.LoadedProjectAsync(action);

            return joinable.Task;
        }

#pragma warning restore RS0030 // Do not use LoadedProjectAsync

        public Task<T> PrioritizedProjectLoadedInHostAsync<T>(Func<Task<T>> action)
        {
            Requires.NotNull(action, nameof(action));

            _tasksService.UnloadCancellationToken.ThrowIfCancellationRequested();

            JoinableTask<T> task = _threadingService.JoinableTaskFactory.RunAsync(action);

            _prioritizedTasks.Add(task);

            return task.Task;
        }

        public Task PrioritizedProjectLoadedInHostAsync(Func<Task> action)
        {
            Requires.NotNull(action, nameof(action));

            _tasksService.UnloadCancellationToken.ThrowIfCancellationRequested();

            JoinableTask task = _threadingService.JoinableTaskFactory.RunAsync(action);

            _prioritizedTasks.Add(task);

            return task.Task;
        }

        public void OnProjectLoadedInHost()
        {
            _projectLoadedInHost.SetResult(null);
        }

        public void OnPrioritizedProjectLoadedInHost()
        {
            _prioritizedProjectLoadedInHost.SetResult(null);

            _threadingService.ExecuteSynchronously(() => _prioritizedTasks.JoinTillEmptyAsync());
        }
    }
}
