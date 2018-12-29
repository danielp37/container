﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Injection;
using Unity.Storage;

namespace Unity.Processors
{
    public partial class ConstructorProcessor : MethodBaseInfoProcessor<ConstructorInfo>
    {
        #region Fields

        private readonly Func<Type, bool> _isTypeRegistered;
        private static readonly TypeInfo _delegateType = typeof(Delegate).GetTypeInfo();

        #endregion


        #region Constructors

        public ConstructorProcessor(IPolicySet policySet, Func<Type, bool> isTypeRegistered)
            : base(policySet, typeof(InjectionConstructorAttribute))
        {
            _isTypeRegistered = isTypeRegistered;
            SelectMethod = SmartSelector;
        }

        #endregion


        #region Public Properties

        public Func<Type, ConstructorInfo[], object> SelectMethod { get; set; }
        
        #endregion


        #region Overrides

        protected override ConstructorInfo[] DeclaredMembers(Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo()
                       .DeclaredConstructors
                       .Where(c => c.IsStatic == false && c.IsPublic)
                       .ToArray();
#else
            return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                       .ToArray();
#endif
        }

        protected override IEnumerable<object> SelectMembers(Type type, ConstructorInfo[] members, InjectionMember[] injectors)
        {
            // Select Injected Members
            if (null != injectors)
            {
                foreach (var injectionMember in injectors)
                {
                    if (injectionMember is InjectionMember<ConstructorInfo, object[]>)
                    {
                        yield return injectionMember;
                        yield break;
                    }
                }
            }

            if (null == members || 0 == members.Length) yield break;
            if (1 == members.Length)
            {
                yield return members[0];
                yield break;
            }

            // Select Attributed members
            foreach (var member in members)
            {
                foreach (var pair in ResolverFactories)
                {
                    if (!member.IsDefined(pair.type)) continue;

                    yield return member;
                    yield break;
                }
            }

            // Select default
            yield return SelectMethod(type, members);
        }

        #endregion


        #region Implementation                                               

        /// <summary>
        /// Selects default constructor
        /// </summary>
        /// <param name="type"><see cref="Type"/> to be built</param>
        /// <param name="members">All public constructors this type implements</param>
        /// <returns></returns>
        public object LegacySelector(Type type, ConstructorInfo[] members)
        {
            Array.Sort(members, ConstructorComparer);

            switch (members.Length)
            {
                case 0:
                    return null;

                case 1:
                    return members[0];

                default:
                    var paramLength = members[0].GetParameters().Length;
                    if (members[1].GetParameters().Length == paramLength)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Constants.AmbiguousInjectionConstructor,
                                type.GetTypeInfo().Name,
                                paramLength));
                    }
                    return members[0];
            }
        }

        private object SmartSelector(Type type, ConstructorInfo[] constructors)
        {
            Array.Sort(constructors, (a, b) =>
            {
                var qtd = b.GetParameters().Length.CompareTo(a.GetParameters().Length);



                if (qtd == 0)
                {
#if NETSTANDARD1_0 || NETCOREAPP1_0
                    return b.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.GetTypeInfo().IsInterface ? 1 : 0));
#else
                    return b.GetParameters().Sum(p => p.ParameterType.IsInterface ? 1 : 0)
                        .CompareTo(a.GetParameters().Sum(p => p.ParameterType.IsInterface ? 1 : 0));
#endif
                }
                return qtd;
            });

            int parametersCount = 0;
            ConstructorInfo bestCtor = null;
            HashSet<Type> bestCtorParameters = null;

            foreach (var ctorInfo in constructors)
            {
                var parameters = ctorInfo.GetParameters();

                if (null != bestCtor && parametersCount > parameters.Length) return bestCtor;
                parametersCount = parameters.Length;
#if NET40
                if (parameters.All(p => _container.CanResolve(p.ParameterType) || null != p.DefaultValue && !(p.DefaultValue is DBNull)))
#else
                if (parameters.All(p => p.HasDefaultValue || CanResolve(p.ParameterType)))
#endif
                {
                    if (bestCtor == null)
                    {
                        bestCtor = ctorInfo;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (null == bestCtorParameters)
                        {
                            bestCtorParameters = new HashSet<Type>(
                                bestCtor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestCtorParameters.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                        {
                            if (bestCtorParameters.All(p => p.GetTypeInfo().IsInterface) &&
                                !parameters.All(p => p.ParameterType.GetTypeInfo().IsInterface))
                                return bestCtor;

                            throw new InvalidOperationException($"Failed to select a constructor for {type.FullName}");
                        }

                        return bestCtor;
                    }
                }
            }

            if (bestCtor == null)
            {
                //return null;
                throw new InvalidOperationException(
                    $"Builder not found for { type.FullName}");
            }

            return bestCtor;
        }

        private bool CanResolve(Type type)
        {
            var info = type.GetTypeInfo();

            if (info.IsClass)
            {
                if (_delegateType.IsAssignableFrom(info) ||
                    typeof(string) == type || info.IsEnum || info.IsPrimitive || info.IsAbstract)
                {
                    return _isTypeRegistered(type);
                }

                if (type.IsArray)
                {
                    return _isTypeRegistered(type) || CanResolve(type.GetElementType());
                }

                return true;
            }

            if (info.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(IEnumerable<>) || _isTypeRegistered(genericType))
                {
                    return true;
                }
            }

            return _isTypeRegistered(type);
        }

        #endregion
    }
}