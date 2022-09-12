﻿using Routine.Core;
using System.Collections.Generic;
using System;

namespace Routine.Service.RequestHandlers.Exceptions;

public class AmbiguousModelException : Exception
{
    public List<ObjectModel> AvailableModels { get; }

    public AmbiguousModelException(List<ObjectModel> availableModels)
    {
        AvailableModels = availableModels;
    }
}
