﻿using System;
using System.Linq;
using System.Reflection;
using Unity.Exceptions;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Resolution;

namespace Unity
{
    public partial class ConstructorPipeline
    {
        #region PipelineBuilder

        public override ResolveDelegate<PipelineContext>? Build(ref PipelineBuilder builder)
        {
            var pipeline = builder.Pipeline();

            if (null != builder.SeedMethod) return pipeline;

            // Verify if can build
#if NETSTANDARD1_0 || NETCOREAPP1_0
            if (builder.Type.GetTypeInfo().IsGenericTypeDefinition)
#else
            if (builder.Type.IsGenericTypeDefinition)
#endif
            {
                return (ref PipelineContext context) =>
                {
                    if (null == context.Existing)
                        throw new InvalidRegistrationException(
                            $"The type {context.Type} is an open generic. An open generic type cannot be created.");

                    return null == pipeline ? context.Existing : pipeline.Invoke(ref context);
                };
            }

            // Select ConstructorInfo
            var selector = GetOrDefault(builder.Policies);
            var selection = selector.Invoke(builder.Type, builder.InjectionMembers)
                                    .FirstOrDefault();

            // Select constructor for the Type
            ConstructorInfo info;
            object[]? resolvers = null;

            switch (selection)
            {
                case ConstructorInfo memberInfo:
                    info = memberInfo;
                    break;

                case MethodBase<ConstructorInfo> injectionMember:
                    info = injectionMember.MemberInfo(builder.Type);
                    resolvers = injectionMember.Data;
                    break;

                case Exception exception:
                    return (ref PipelineContext c) =>
                    {
                        if (null == c.Existing)
                            throw exception;

                        return null == pipeline ? c.Existing : pipeline.Invoke(ref c);
                    };

                default:
                    return (ref PipelineContext c) =>
                    {
                        if (null == c.Existing)
                            throw new InvalidRegistrationException($"No public constructor is available for type {c.Type}.");

                        return null == pipeline ? c.Existing : pipeline.Invoke(ref c);
                    };
            }

            return GetResolverDelegate(info, resolvers, pipeline, builder.LifetimeManager is PerResolveLifetimeManager);
        }

        #endregion


        #region Implementation

        protected virtual ResolveDelegate<PipelineContext> GetResolverDelegate(ConstructorInfo info, object? resolvers, 
                                                                               ResolveDelegate<PipelineContext>? pipeline, 
                                                                               bool perResolve)
        {
            try
            {
                // Create parameter resolvers
                var parameterResolvers = ParameterResolvers(info.GetParameters(), resolvers).ToArray();

                // Select Lifetime type ( Per Resolve )
                if (perResolve)
                {
                    // Activate type
                    return (ref PipelineContext context) =>
                    {
                        if (null == context.Existing)
                        {
                            var dependencies = new object[parameterResolvers.Length];
                            for (var i = 0; i < dependencies.Length; i++)
                                dependencies[i] = parameterResolvers[i](ref context);

                            context.Existing = info.Invoke(dependencies);
                            context.Set(typeof(LifetimeManager), new RuntimePerResolveLifetimeManager(context.Existing));
                        }

                        // Invoke other initializers
                        return null == pipeline ? context.Existing : pipeline.Invoke(ref context);
                    };
                }
                else
                {
                    // Activate type
                    return (ref PipelineContext context) =>
                    {
                        if (null == context.Existing)
                        {
                            var dependencies = new object[parameterResolvers.Length];
                            for (var i = 0; i < dependencies.Length; i++)
                                dependencies[i] = parameterResolvers[i](ref context);

                            context.Existing = info.Invoke(dependencies);
                        }

                        // Invoke other initializers
                        return null == pipeline ? context.Existing : pipeline.Invoke(ref context);
                    };
                }
            }
            catch (InvalidRegistrationException ex)
            {
                // Throw if try to create
                return (ref PipelineContext context) =>
                {
                    if (null == context.Existing) throw ex;
                    return null == pipeline ? context.Existing : pipeline.Invoke(ref context);
                };
            }
            catch (Exception ex)
            {
                // Throw if try to create
                return (ref PipelineContext context) =>
                {
                    if (null == context.Existing) throw new InvalidRegistrationException(ex);
                    return null == pipeline ? context.Existing : pipeline.Invoke(ref context);
                };
            }
        }

        #endregion
    }
}
