﻿using System;
using Unity.Resolution;

namespace Unity
{
    public delegate ResolveDelegate<PipelineContext> TypeFactoryDelegate(Type type, UnityContainer container);
}
