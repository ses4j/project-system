﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem
{
    /// <summary>
    ///     An <see langword="abstract"/> base class that simplifies the lifetime of 
    ///     a component that is loaded and unloaded multiple times.
    /// </summary>
    internal abstract class AbstractMultiLifetimeComponent<T> : OnceInitializedOnceDisposedAsync
        where T : class, IMultiLifetimeInstance
    {
        private readonly object _lock = new object();
        private TaskCompletionSource<(T instance, JoinableTask initializeAsyncTask)> _instanceTaskSource = new TaskCompletionSource<(T instance, JoinableTask initializeAsyncTask)>(TaskCreationOptions.RunContinuationsAsynchronously);

        protected AbstractMultiLifetimeComponent(JoinableTaskContextNode joinableTaskContextNode)
             : base(joinableTaskContextNode)
        {
        }

        /// <summary>
        ///     Returns a task that will complete when current <see cref="AbstractMultiLifetimeComponent{T}"/> has completed
        ///     loading and has published its instance.
        /// </summary>
        /// <exception cref="OperationCanceledException">
        ///     The result is awaited and the <see cref="ConfiguredProject"/> is unloaded.
        ///     <para>
        ///         -or
        ///     </para>
        ///     The result is awaited and <paramref name="cancellationToken"/> is cancelled.
        /// </exception>
        /// <remarks>
        ///     This method does not initiate loading of the <see cref="AbstractMultiLifetimeComponent{T}"/>, however,
        ///     it will join the load when it starts.
        /// </remarks>
        protected async Task<T> WaitForLoadedAsync(CancellationToken cancellationToken = default)
        {
            // Wait until LoadAsync has been called, force switching to thread-pool in case
            // there's already someone waiting for us on the UI thread.
#pragma warning disable RS0030 // Do not used banned APIs
            (T instance, JoinableTask initializeAsyncTask) = await _instanceTaskSource.Task.WithCancellation(cancellationToken)
                                                                                           .ConfigureAwait(false);
#pragma warning restore RS0030

            // Now join Instance.InitializeAsync so that if someone is waiting on the UI thread for us, 
            // the instance is allowed to switch to that thread to complete if needed.
            await initializeAsyncTask.JoinAsync(cancellationToken);

            return instance;
        }

        public async Task LoadAsync()
        {
            await InitializeAsync();

            await LoadCoreAsync();
        }

        public async Task LoadCoreAsync()
        {
            JoinableTask initializeAsyncTask;

            lock (_lock)
            {
                if (!_instanceTaskSource.Task.IsCompleted)
                {
                    (T instance, JoinableTask initializeAsyncTask) result = CreateInitializedInstanceAsync();
                    _instanceTaskSource.SetResult(result);
                }

                Assumes.True(_instanceTaskSource.Task.IsCompleted);

                // Should throw TaskCanceledException if already cancelled in Dispose
                (_, initializeAsyncTask) = _instanceTaskSource.Task.GetAwaiter().GetResult();
            }

            await initializeAsyncTask;
        }

        public Task UnloadAsync()
        {
            T instance = null;
            lock (_lock)
            {
                if (_instanceTaskSource.Task.IsCompleted)
                {
                    // Should throw TaskCanceledException if already cancelled in Dispose
                    (instance, _) = _instanceTaskSource.Task.GetAwaiter().GetResult();
                    _instanceTaskSource = new TaskCompletionSource<(T instance, JoinableTask initializeAsyncTask)>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }

            if (instance != null)
            {
                return instance.DisposeAsync();
            }

            return Task.CompletedTask;
        }

        protected override async Task DisposeCoreAsync(bool initialized)
        {
            await UnloadAsync();

            lock (_lock)
            {
                _instanceTaskSource.TrySetCanceled();
            }
        }

        protected override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Creates a new instance of the underlying <see cref="IMultiLifetimeInstance"/>.
        /// </summary>
        protected abstract T CreateInstance();

        private (T instance, JoinableTask initializeAsyncTask) CreateInitializedInstanceAsync()
        {
            T instance = CreateInstance();

            JoinableTask initializeAsyncTask = JoinableFactory.RunAsync(instance.InitializeAsync);

            return (instance, initializeAsyncTask);
        }
    }
}
