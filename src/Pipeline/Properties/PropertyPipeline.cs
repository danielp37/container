﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Policy;
using Unity.Resolution;

namespace Unity
{
    public class PropertyPipeline : MemberPipeline<PropertyInfo, object>
    {
        #region Constructors

        public PropertyPipeline(UnityContainer container)
            : base(container)
        {
        }

        #endregion


        #region Overrides

        protected override Type MemberType(PropertyInfo info) => info.PropertyType;

        protected override IEnumerable<PropertyInfo> DeclaredMembers(Type type)
        {
            foreach (var member in type.GetDeclaredProperties())
            {
                if (!member.CanWrite || 0 != member.GetIndexParameters().Length)
                    continue;

                var setter = member.GetSetMethod(true);
                if (setter.IsPrivate || setter.IsFamily)
                    continue;

                yield return member;
            }
        }

        public override MemberSelector<PropertyInfo> GetOrDefault(IPolicySet? registration) =>
            registration?.Get<MemberSelector<PropertyInfo>>() ?? Defaults.SelectProperty;

        #endregion


        #region Resolution

        protected override ResolveDelegate<PipelineContext> GetResolverDelegate(PropertyInfo info, object? resolver)
        {
            var value = PreProcessResolver(info, resolver);
            return (ref PipelineContext context) =>
            {
#if NET40
                info.SetValue(context.Existing, context.Resolve(info, value), null);
#else
                info.SetValue(context.Existing, context.Resolve(info, value));
#endif
                return context.Existing;
            };
        }

        #endregion


        #region Expression 

        protected override Expression GetResolverExpression(PropertyInfo info, object? resolver)
        {
            return Expression.Assign(
                Expression.Property(Expression.Convert(PipelineContextExpression.Existing, info.DeclaringType), info),
                Expression.Convert(
                    Expression.Call(PipelineContextExpression.Context,
                        PipelineContextExpression.ResolvePropertyMethod,
                        Expression.Constant(info, typeof(PropertyInfo)),
                        Expression.Constant(PreProcessResolver(info, resolver), typeof(object))),
                    info.PropertyType));
        }

        #endregion
    }
}
