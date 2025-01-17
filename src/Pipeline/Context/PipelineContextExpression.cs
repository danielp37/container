﻿using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Resolution;

namespace Unity
{
    public class PipelineContextExpression : IResolveContextExpression<PipelineContext>
    {
        #region Fields

        public static readonly MethodInfo ResolvePropertyMethod =
            typeof(PipelineContext).GetTypeInfo()
                .GetDeclaredMethods(nameof(PipelineContext.Resolve))
                .First(m =>
                {
                    var parameters = m.GetParameters();
                    return 0 < parameters.Length &&
                        typeof(PropertyInfo) == parameters[0].ParameterType;
                });

        public static readonly MethodInfo ResolveFieldMethod =
            typeof(PipelineContext).GetTypeInfo()
                .GetDeclaredMethods(nameof(PipelineContext.Resolve))
                .First(m =>
                {
                    var parameters = m.GetParameters();
                    return 0 < parameters.Length &&
                        typeof(FieldInfo) == parameters[0].ParameterType;
                });

        public static readonly MethodInfo ResolveParameterMethod =
            typeof(PipelineContext).GetTypeInfo()
                .GetDeclaredMethods(nameof(PipelineContext.Resolve))
                .First(m =>
                {
                    var parameters = m.GetParameters();
                    return 0 < parameters.Length &&
                        typeof(ParameterInfo) == parameters[0].ParameterType;
                });

        public static readonly MethodInfo SetMethod =
            typeof(PipelineContext).GetTypeInfo()
                .GetDeclaredMethods(nameof(PipelineContext.Set))
                .First(m => 2 == m.GetParameters().Length);

        public static readonly MethodInfo GetMethod =
            typeof(PipelineContext).GetTypeInfo()
                .GetDeclaredMethods(nameof(PipelineContext.Get))
                .First(m => 1 == m.GetParameters().Length);

        #endregion


        #region Constructor

        static PipelineContextExpression()
        {
            var typeInfo = typeof(PipelineContext).GetTypeInfo();

            Parent            = Expression.MakeMemberAccess(Context, typeInfo.GetDeclaredField(nameof(PipelineContext.Parent)));
            Existing          = Expression.MakeMemberAccess(Context, typeInfo.GetDeclaredProperty(nameof(PipelineContext.Existing)));
            DeclaringType     = Expression.MakeMemberAccess(Context, typeInfo.GetDeclaredField(nameof(PipelineContext.DeclaringType)));
            LifetimeContainer = Expression.MakeMemberAccess(Context, typeInfo.GetDeclaredProperty(nameof(PipelineContext.LifetimeContainer)));
        }

        #endregion


        #region Public Properties

        public static readonly MemberExpression Parent;

        public static readonly MemberExpression Existing;

        public static readonly MemberExpression DeclaringType;

        public static readonly MemberExpression LifetimeContainer;

        #endregion
    }
}
