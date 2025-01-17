﻿using System;
using System.Linq;
using System.Reflection;
using Unity.Resolution;

namespace Unity.Factories
{
    public class ArrayResolver
    {
        #region Fields

        private static readonly MethodInfo ResolverMethod =
            typeof(ArrayResolver).GetTypeInfo()
                                 .GetDeclaredMethod(nameof(ArrayResolver.ResolverFactory));

        private static readonly MethodInfo BuiltInMethod =
            typeof(ArrayResolver).GetTypeInfo()
                                 .GetDeclaredMethod(nameof(ArrayResolver.BuiltInFactory));
        #endregion


        #region TypeResolverFactory

        public static TypeFactoryDelegate Factory = (Type type, UnityContainer container) =>
        {
            var typeArgument = type.GetElementType();
            var targetType = container.GetTargetType(typeArgument);

            if (null != targetType && typeArgument != targetType)
            {
                return ((BuiltInFactoryDelegate)BuiltInMethod
                    .MakeGenericMethod(typeArgument)
                    .CreateDelegate(typeof(BuiltInFactoryDelegate)))(targetType);
            }

            return ((ArrayFactoryDelegate)ResolverMethod
                .MakeGenericMethod(typeArgument)
                .CreateDelegate(typeof(ArrayFactoryDelegate)))();
        };

        #endregion


        #region Implementation

        private static ResolveDelegate<PipelineContext> ResolverFactory<TElement>()
        {
#if NETSTANDARD1_0 || NETCOREAPP1_0
            if (typeof(TElement).GetTypeInfo().IsGenericType)
#else
            if (typeof(TElement).IsGenericType)
#endif
            {
                var definition = typeof(TElement).GetGenericTypeDefinition();
                return (ref PipelineContext c) => ((UnityContainer)c.Container).ResolveArray<TElement>(c.Resolve, typeof(TElement), definition)
                                                                              .ToArray();
            }

            return (ref PipelineContext c) => ((UnityContainer)c.Container).ResolveArray<TElement>(c.Resolve, typeof(TElement))
                                                                          .ToArray();
        }

        private static ResolveDelegate<PipelineContext> BuiltInFactory<TElement>(Type type)
        {
#if NETSTANDARD1_0 || NETCOREAPP1_0
            var info = type.GetTypeInfo();
            if (info.IsGenericType && !info.IsGenericTypeDefinition)
#else
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
#endif
            {
                var definition = type.GetGenericTypeDefinition();
                return (ref PipelineContext c) => ((UnityContainer)c.Container).ComplexArray<TElement>(c.Resolve, type, definition)
                                                                              .ToArray();
            }

            return (ref PipelineContext c) => ((UnityContainer)c.Container).ComplexArray<TElement>(c.Resolve, type)
                                                                          .ToArray();
        }

        #endregion


        #region Nested Types

        private delegate ResolveDelegate<PipelineContext> ArrayFactoryDelegate();
        private delegate ResolveDelegate<PipelineContext> BuiltInFactoryDelegate(Type type);

        #endregion
    }
}
