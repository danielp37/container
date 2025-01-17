﻿using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Exceptions;
using Unity.Factories;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class FactoryPipeline : Pipeline
    {
        public override ResolveDelegate<PipelineContext>? Build(ref PipelineBuilder builder)
        {
            // Skip if already have a resolver
            if (null != builder.SeedMethod) return builder.Pipeline();

            var registration = builder.Registration as FactoryRegistration ??
                               builder.Factory      as FactoryRegistration;

            Debug.Assert(null != registration);
            var factory = registration.Factory;

            return builder.Pipeline((ref PipelineContext context) => factory(context.Container, context.Type, context.Name));
        }
    }
}
