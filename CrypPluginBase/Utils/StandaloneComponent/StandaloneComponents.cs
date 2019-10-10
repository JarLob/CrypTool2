﻿/*
   Copyright 2019 Simon Leischnig, based on the work of Soeren Rinne

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using Cryptool.PluginBase.Utils.Datatypes; using static Cryptool.PluginBase.Utils.Datatypes.Datatypes;
using Cryptool.PluginBase.Utils.Logging;
using Cryptool.PluginBase.Utils.StandaloneComponent.Common;

namespace Cryptool.PluginBase.Utils.StandaloneComponent
{
    using Cryptool.PluginBase.Utils.Logging;
    using Cryptool.PluginBase.Utils.StandaloneComponent.Common;
    using System.Windows.Controls;

    // Parameters (Settings) (perfunctory, for easier adaption of tighter modeling later)
    public interface IParameters
    {
    }
    public abstract class AbstractParameters : IParameters
    {
        public AbstractParameters() { }
    }

    // Component model
    public interface IStandaloneComponent<ApiType, ParamsType>
        where ApiType : IComponentAPI<ParamsType>
        where ParamsType : IParameters
    {
        ApiType api { get; }
    }
    public abstract class AbstractStandaloneComponent<ApiType, ParamsType>
        : IStandaloneComponent<ApiType, ParamsType>
        where ApiType : IComponentAPI<ParamsType>
        where ParamsType : IParameters
    {
        public abstract UserControl Presentation { get; }
        public ApiType api { get; }

        public AbstractStandaloneComponent(ApiType api)
        {
            this.api = api;
        }

    }

    // API
    public interface IComponentAPI<ParamsType> where ParamsType : IParameters
    {
        ParamsType Parameters { get; }

        event Action OnDispose;
        event Action OnExecute;
        event Action OnInitialize;
        event Action OnPostExecution;
        event Action OnPreExecution;
        event Action OnStop;

        void _raiseDispose();
        void _raiseExecute();
        void _raiseInitialize();
        void _raisePostExecution();
        void _raisePreExecution();
        void _raiseStop();

        event Action<int> OnStatusImageChanged;
        event Action<ComponentProgress> OnProgressChanged;
        event Action<string, LogLevel> OnLogMessage;
    }
    public abstract class AbstractComponentAPI<ParamsType> : IComponentAPI<ParamsType> where ParamsType : IParameters
    {

        public ParamsType Parameters { get; }

        public event Action OnDispose = () => { };
        public event Action OnExecute = () => { };
        public event Action OnInitialize = () => { };
        public event Action OnPostExecution = () => { };
        public event Action OnPreExecution = () => { };
        public event Action OnStop = () => { };

        public event Action<ComponentProgress> OnProgressChanged = (progress) => { };

        public void _raiseDispose() { this.OnDispose(); }
        public void _raiseExecute() { this.OnExecute(); }
        public void _raiseInitialize() { this.OnInitialize(); }
        public void _raisePostExecution() { this.OnPostExecution(); }
        public void _raisePreExecution() { this.OnPreExecution(); }
        public void _raiseStop() { this.OnStop(); }

        public event Action<int> OnStatusImageChanged = (status) => { };
        public event Action<string, LogLevel> OnLogMessage = (msg, LogLevel) => { };

        protected void ChangeProgress(double ratio, params object[] context)
        {
            if (ratio > 0.999)
            {
                this.OnProgressChanged(new ComponentProgress(ComponentProgress.Kinds.Finished, ratio));
            }
            else
            {
                this.OnProgressChanged(new ComponentProgress(ComponentProgress.Kinds.Pending, ratio));
            }
        }
        protected void ChangeProgressToFinished(params object[] context) => this.OnProgressChanged(new ComponentProgress(ComponentProgress.Kinds.Finished, 1.0, context));
        protected void ChangeStateToWontcomplete(params object[] context) => this.OnProgressChanged(new ComponentProgress(ComponentProgress.Kinds.Wontfinish, 0.0, context));

        protected void ChangeStatusImage(int status) => this.OnStatusImageChanged(status);
        public void Log(string msg, LogLevel lvl) => this.OnLogMessage(msg, lvl);

        public AbstractComponentAPI(ParamsType parameters)
        {
            this.Parameters = parameters;
            this.OnPreExecution += () => this.ChangeProgress(0);
        }

    }
}

namespace Cryptool.PluginBase.Utils.StandaloneComponent.Common
{
    public class ComponentProgress
    {
        public enum Kinds { Pending, Finished, Wontfinish }
        public Kinds Kind { get; }
        public double Ratio { get; }
        public object[] Context { get; }
        public bool IsFinished { get => Kind == Kinds.Finished || Kind == Kinds.Wontfinish; }

        public ComponentProgress(Kinds kind, double ratio, params object[] context)
        {
            Kind = kind;
            Ratio = ratio;
            Context = context;
        }
    }

    public interface IParameter<T>
    {
        T Value { get; set; }
        event Action<T> OnChange;
    }
    public abstract class AbstractParameter<T> : IParameter<T>
    {
        protected Box<T> container;
        public T Value { get => container.Value; set => container.Value = value; }
        public event Action<T> OnChange = (value) => { };
        public void Set(T value) => this.Value = value;

        public AbstractParameter(T initialValue)
        {
            this.container = new Box<T>(initialValue);
            this.container.OnChange += value => this.OnChange(value);
        }
        public AbstractParameter() : this(default(T)) { }

    }
    public class Parameter<T> : AbstractParameter<T>
    {
        public Parameter() : base() { }
        public Parameter(T initialValue) : base(initialValue) { }
        public static implicit operator T(Parameter<T> param) => param.Value;
    }
}


