﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Tools.BuildLogging.UI;
using Microsoft.VisualStudio.ProjectSystem.Tools.Providers;
using Microsoft.VisualStudio.ProjectSystem.Tools.Providers.RpcContracts;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.ProjectSystem.Tools.BuildLogging.Model
{
    [Export(typeof(IFrontendBuildTableDataSource))]
    internal sealed class FrontendBuildTableDataSource : ITableEntriesSnapshotFactory, IFrontendBuildTableDataSource
    {
        private const string BuildDataSourceDisplayName = "Build Data Source";
        private const string BuildTableDataSourceIdentifier = nameof(BuildTableDataSourceIdentifier);
        private const string BuildTableDataSourceSourceTypeIdentifier = nameof(BuildTableDataSourceSourceTypeIdentifier);

        private readonly object _gate = new object();

        private ITableManager _manager;
        private ITableDataSink _tableDataSink;
        private BuildTableEntriesSnapshot _lastSnapshot;
        private ImmutableList<UIBuildSummary> _entries = ImmutableList<UIBuildSummary>.Empty;

        private readonly IBuildLoggerService _loggerService;

        public string SourceTypeIdentifier => BuildTableDataSourceSourceTypeIdentifier;

        public string Identifier => BuildTableDataSourceIdentifier;

        public string DisplayName => BuildDataSourceDisplayName;

        // TODO: Figure out the syntax to await this information from the server
        //public bool SupportRoslynLogging => _roslynLogger.Supported;
        public bool SupportRoslynLogging => false;

        public int CurrentVersionNumber { get; private set; }

        public ITableManager Manager
        {
            get => _manager;
            set
            {
                _manager?.RemoveSource(this);
                _manager = value;
                _manager?.AddSource(this);
            }
        }

        [ImportingConstructor]
        public FrontendBuildTableDataSource(IBuildLoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public bool IsLogging()
        {
            return _loggerService.IsLogging();
            //Task<bool> taskResult = 
            //bool result = await taskResult;
            //return result;
        }

        public void Start()
        {
            _loggerService.Start();
            //Task<bool> taskResult = 
            //bool result = await taskResult;
            //return result;
        }

        public void Stop()
        {
            UpdateEntries();
            _loggerService.Stop();
            //Task<bool> taskResult = 
            //bool result = await taskResult;
            //return result;
        }

        public void Clear()
        {
            _loggerService.Clear();
            //Task<bool> taskResult = 
            foreach (var build in _entries)
            {
                // TODO: How does this change later in server context
                //build.Dispose();
            }
            _entries = ImmutableList<UIBuildSummary>.Empty;
            CurrentVersionNumber++;
            NotifyChange();
            //bool result = await taskResult;
            //return result;
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            _tableDataSink = sink;

            _tableDataSink.AddFactory(this, removeAllFactories: true);
            _tableDataSink.IsStable = true;

            return this;
        }

        public void Dispose()
        {
            foreach (var build in _entries)
            {
                // TODO: How does this change later in server context
                //build.Dispose();
            }
            _entries = ImmutableList<UIBuildSummary>.Empty;
            Manager = null;
        }

        public void NotifyChange()
        {
            CurrentVersionNumber++;
            _tableDataSink.FactorySnapshotChanged(this);
        }

        public ITableEntriesSnapshot GetCurrentSnapshot()
        {
            lock (_gate)
            {
                if (_lastSnapshot?.VersionNumber != CurrentVersionNumber)
                {
                    _lastSnapshot = new BuildTableEntriesSnapshot(_entries, CurrentVersionNumber);
                }

                return _lastSnapshot;
            }
        }

        public ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            lock (_gate)
            {
                if (_lastSnapshot?.VersionNumber == versionNumber)
                {
                    return _lastSnapshot;
                }

                if (versionNumber == CurrentVersionNumber)
                {
                    return GetCurrentSnapshot();
                }
            }

            // We didn't have this version.  Notify the sinks that something must have changed
            // so that they call back into us with the latest version.
            NotifyChange();
            return null;
        }

        //public void AddEntry(Build build)
        //{
        //    _entries = _entries.Add(build);
        //    NotifyChange();
        //}

        private void UpdateEntries()
        {
            ImmutableList<BuildSummary> newData = _loggerService.RetrieveAllBuilds();
            _entries = ImmutableList<UIBuildSummary>.Empty;
            foreach (BuildSummary summary in newData)
            {
                _entries = _entries.Add(new UIBuildSummary(summary));
            }
            NotifyChange();
        }
    }
}