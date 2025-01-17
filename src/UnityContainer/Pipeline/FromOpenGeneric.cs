﻿using System;
using System.Diagnostics;
using System.Threading;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        private FromOpenGeneric PipelineFromOpenGeneric { get; }


        #region Optimized

        private ResolveDelegate<PipelineContext> PipelineFromOpenGenericOptimized(ref HashKey key, ExplicitRegistration factory)
        {
            Debug.Assert(null != _registry);
            Debug.Assert(null != key.Type);

            LifetimeManager? manager = null;
            ResolveDelegate<PipelineContext>? pipeline = null;

            // Add Pipeline to the Registry
            lock (_syncRegistry)
            {
                bool adding = true;
                var collisions = 0;
                var targetBucket = key.HashCode % _registry.Buckets.Length;
                for (var i = _registry.Buckets[targetBucket]; i >= 0; i = _registry.Entries[i].Next)
                {
                    ref var candidate = ref _registry.Entries[i];
                    if (candidate.Key != key)
                    {
                        collisions++;
                        continue;
                    }

                    // Pipeline already been created
                    if (null != candidate.Pipeline) return candidate.Pipeline;

                    // Lifetime Manager
                    manager = Context.TypeLifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Type has not been registered
                    if (null == candidate.Registration) candidate.Pipeline = manager.Pipeline;

                    adding = false;
                    break;
                }

                if (adding)
                {
                    // Expand if required
                    if (_registry.RequireToGrow || CollisionsCutPoint < collisions)
                    {
                        _registry = new Registry(_registry);
                        targetBucket = key.HashCode % _registry.Buckets.Length;
                    }

                    // Lifetime Manager
                    manager = factory.LifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Create new entry
                    ref var entry = ref _registry.Entries[_registry.Count];
                    entry.Key = key;
                    entry.Pipeline = manager.Pipeline;
                    entry.Next = _registry.Buckets[targetBucket];
                    _registry.Buckets[targetBucket] = _registry.Count++;
                }

                if (manager is IDisposable disposable) LifetimeContainer.Add(disposable);
            }

            Debug.Assert(null != manager);

            lock (manager)
            {
                if ((Delegate)(ResolveDelegate<PipelineContext>)SpinWait == manager.PipelineDelegate)
                {
                    PipelineBuilder builder = new PipelineBuilder(key.Type, factory, manager, this);
                    manager.PipelineDelegate = builder.Pipeline();
                    Debug.Assert(null != manager.PipelineDelegate);
                    pipeline = (ResolveDelegate<PipelineContext>)manager.PipelineDelegate;
                }
            }

            return manager.Pipeline;


            object? SpinWait(ref PipelineContext context)
            {
                while (null == pipeline)
                {
#if NETSTANDARD1_0 || NETCOREAPP1_0
                    for (var i = 0; i < 100; i++) ;
#else
                    Thread.SpinWait(100);
#endif
                }

                return pipeline(ref context);
            }
        }

        #endregion


        #region Activated

        private ResolveDelegate<PipelineContext> PipelineFromOpenGenericActivated(ref HashKey key, ExplicitRegistration factory)
        {
            Debug.Assert(null != _registry);
            Debug.Assert(null != key.Type);

            LifetimeManager? manager = null;
            ResolveDelegate<PipelineContext>? pipeline = null;

            // Add Pipeline to the Registry
            lock (_syncRegistry)
            {
                bool adding = true;
                var collisions = 0;
                var targetBucket = key.HashCode % _registry.Buckets.Length;
                for (var i = _registry.Buckets[targetBucket]; i >= 0; i = _registry.Entries[i].Next)
                {
                    ref var candidate = ref _registry.Entries[i];
                    if (candidate.Key != key)
                    {
                        collisions++;
                        continue;
                    }

                    // Pipeline already been created
                    if (null != candidate.Pipeline) return candidate.Pipeline;

                    // Lifetime Manager
                    manager = Context.TypeLifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Type has not been registered
                    if (null == candidate.Registration) candidate.Pipeline = manager.Pipeline;

                    adding = false;
                    break;
                }

                if (adding)
                {
                    // Expand if required
                    if (_registry.RequireToGrow || CollisionsCutPoint < collisions)
                    {
                        _registry = new Registry(_registry);
                        targetBucket = key.HashCode % _registry.Buckets.Length;
                    }

                    // Lifetime Manager
                    manager = factory.LifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Create new entry
                    ref var entry = ref _registry.Entries[_registry.Count];
                    entry.Key = key;
                    entry.Pipeline = manager.Pipeline;
                    entry.Next = _registry.Buckets[targetBucket];
                    _registry.Buckets[targetBucket] = _registry.Count++;
                }

                if (manager is IDisposable disposable) LifetimeContainer.Add(disposable);
            }

            Debug.Assert(null != manager);

            lock (manager)
            {
                if ((Delegate)(ResolveDelegate<PipelineContext>)SpinWait == manager.PipelineDelegate)
                {
                    PipelineBuilder builder = new PipelineBuilder(key.Type, factory, manager, this);
                    manager.PipelineDelegate = builder.Pipeline();
                    Debug.Assert(null != manager.PipelineDelegate);
                    pipeline = (ResolveDelegate<PipelineContext>)manager.PipelineDelegate;
                }
            }

            return manager.Pipeline;


            object? SpinWait(ref PipelineContext context)
            {
                while (null == pipeline)
                {
#if NETSTANDARD1_0 || NETCOREAPP1_0
                    for (var i = 0; i < 100; i++) ;
#else
                    Thread.SpinWait(100);
#endif
                }

                return pipeline(ref context);
            }
        }

        #endregion


        #region Compiled

        private ResolveDelegate<PipelineContext> PipelineFromOpenGenericCompiled(ref HashKey key, ExplicitRegistration factory)
        {
            Debug.Assert(null != _registry);
            Debug.Assert(null != key.Type);

            LifetimeManager? manager = null;
            ResolveDelegate<PipelineContext>? pipeline = null;

            // Add Pipeline to the Registry
            lock (_syncRegistry)
            {
                bool adding = true;
                var collisions = 0;
                var targetBucket = key.HashCode % _registry.Buckets.Length;
                for (var i = _registry.Buckets[targetBucket]; i >= 0; i = _registry.Entries[i].Next)
                {
                    ref var candidate = ref _registry.Entries[i];
                    if (candidate.Key != key)
                    {
                        collisions++;
                        continue;
                    }

                    // Pipeline already been created
                    if (null != candidate.Pipeline) return candidate.Pipeline;

                    // Lifetime Manager
                    manager = Context.TypeLifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Type has not been registered
                    if (null == candidate.Registration) candidate.Pipeline = manager.Pipeline;

                    adding = false;
                    break;
                }

                if (adding)
                {
                    // Expand if required
                    if (_registry.RequireToGrow || CollisionsCutPoint < collisions)
                    {
                        _registry = new Registry(_registry);
                        targetBucket = key.HashCode % _registry.Buckets.Length;
                    }

                    // Lifetime Manager
                    manager = factory.LifetimeManager.CreateLifetimePolicy();
                    manager.PipelineDelegate = (ResolveDelegate<PipelineContext>)SpinWait;

                    // Create new entry
                    ref var entry = ref _registry.Entries[_registry.Count];
                    entry.Key = key;
                    entry.Pipeline = manager.Pipeline;
                    entry.Next = _registry.Buckets[targetBucket];
                    _registry.Buckets[targetBucket] = _registry.Count++;
                }

                if (manager is IDisposable disposable) LifetimeContainer.Add(disposable);
            }

            Debug.Assert(null != manager);

            lock (manager)
            {
                if ((Delegate)(ResolveDelegate<PipelineContext>)SpinWait == manager.PipelineDelegate)
                {
                    PipelineBuilder builder = new PipelineBuilder(key.Type, factory, manager, this);
                    manager.PipelineDelegate = builder.Compile();
                    Debug.Assert(null != manager.PipelineDelegate);
                    pipeline = (ResolveDelegate<PipelineContext>)manager.PipelineDelegate;
                }
            }

            return manager.Pipeline;


            object? SpinWait(ref PipelineContext context)
            {
                while (null == pipeline)
                {
#if NETSTANDARD1_0 || NETCOREAPP1_0
                    for (var i = 0; i < 100; i++) ;
#else
                    Thread.SpinWait(100);
#endif
                }

                return pipeline(ref context);
            }
        }

        #endregion
    }
}
