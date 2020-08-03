﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.Tools.Providers;

namespace Microsoft.VisualStudio.ProjectSystem.Tools.BuildLogging.Model.Backend
{
    /// <summary>
    /// Immutable Type
    /// </summary>
    public sealed class BuildSummary : IBuildSummary
    {
        public int BuildId { get; }

        public BuildType BuildType { get; }

        public IEnumerable<string> Dimensions { get; }

        public IEnumerable<string> Targets { get; }

        public DateTime StartTime { get; }

        public TimeSpan Elapsed { get; }

        public BuildStatus Status { get; }

        public string ProjectPath { get; }

        public BuildSummary(int buildId, string projectPath, IEnumerable<string> dimensions, IEnumerable<string> targets, BuildType buildType, DateTime startTime)
        {
            BuildId = buildId;
            ProjectPath = projectPath;
            Dimensions = dimensions.ToArray();
            Targets = targets?.ToArray() ?? Enumerable.Empty<string>();
            BuildType = buildType;
            StartTime = startTime;
            Status = BuildStatus.Running;
        }
        public BuildSummary(IBuildSummary other, BuildStatus status, TimeSpan elapsed) {
            BuildId = other.BuildId;
            BuildType = other.BuildType;

            Dimensions = other.Dimensions;
            Targets = other.Targets;

            StartTime = other.StartTime;
            ProjectPath = other.ProjectPath;

            Elapsed = elapsed;
            Status = status;
        }
    }
}