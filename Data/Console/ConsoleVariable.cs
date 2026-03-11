
// SIGNATURE :)

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data.Console {

    public class ConsoleVariable : SaveableData, IKwArg {

        private readonly IKwArg _wrappedValue;
        private readonly Action _changeCallback;

        public ConsoleVariable(IKwArg wrappedValue) {
            _wrappedValue = wrappedValue;
            _changeCallback = null;
        }

        public ConsoleVariable(IKwArg wrappedValue, Action callback) {
            _wrappedValue = wrappedValue;
            _changeCallback = callback;
        }

        public override SaveableData DeserializeFrom(string json) {
            float old = GetFloat();
            JsonUtility.FromJsonOverwrite(json, _wrappedValue);
            if(INFRA.Game.IsLoading && !Mathf.Approximately(old, GetFloat())) 
                Debug.Log($@"Read configured value of {_wrappedValue.GetID()}: '{GetString()}'");
            return this;
        }

        public override string GetSerialized() {
            return JsonUtility.ToJson(_wrappedValue);
        }

        /// <summary>
        /// Gets the string description for this ConsoleVariable.
        /// Descriptions are user-readable strings for display in
        /// the console, and they're populated during registration. 
        /// </summary>
        /// <returns></returns>
        public string GetDescription() {
            return _wrappedValue.GetDescription();
        }

        /// <summary>
        /// Gets the ID of this ConsoleVariable.
        /// Due to stupid by me, this does the exact
        /// same thing as GetID().
        /// </summary>
        /// <returns></returns>
        public override string GetSaveableID() {
            return GetID();
        }

        /// <summary>
        /// Gets the ID of this ConsoleVariable.
        /// Due to stupid by me, this does the exact
        /// same thing as GetSaveableID().
        /// </summary>
        /// <returns></returns>
        public string GetID() {
            return _wrappedValue.GetID();
        }

        /// <summary>
        /// Gets the permissable range for this ConsoleVariable
        /// as an IRangeable object. This object describes
        /// the numerical (or enumerated) states that this
        /// ConsoleVariable's Keyword Argument can be in.
        /// <para/> Returns TRUE if the ConsoleVariable was 
        /// modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public IRangeable GetRange() {
            return _wrappedValue.GetRange();
        }

        /// <summary>
        /// Sets this ConsoleVariable to the provided 
        /// integer / float / string / boolean.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, SetValue will do 
        /// its best to convert between types for you, but this 
        /// may result in bad data.
        /// <para/> Returns TRUE if the ConsoleVariable was 
        /// modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public ValueSetResult SetValue(string value) {
            ValueSetResult result = _wrappedValue.SetValue(value);
            if(result == ValueSetResult.SUCCESS) {
                if(_changeCallback != null) 
                    _changeCallback.Invoke();
            }
            return result;
        }

        /// <summary>
        /// Sets this ConsoleVariable to the provided 
        /// integer / float / string / boolean.
        ///  
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, SetValue will do 
        /// its best to convert between types for you, but this 
        /// may result in bad data.
        /// <para/> Returns TRUE if the ConsoleVariable was 
        /// modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public ValueSetResult SetValue(float value) {
            ValueSetResult result = _wrappedValue.SetValue(value);
            if(result == ValueSetResult.SUCCESS) {
                if(_changeCallback != null) 
                    _changeCallback.Invoke();
            }
            return result;
        }

        /// <summary>
        /// Sets this ConsoleVariable to the provided 
        /// integer / float / string / boolean.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, SetValue will do 
        /// its best to convert between types for you, but this 
        /// may result in bad data.
        /// <para/> Returns TRUE if the ConsoleVariable was 
        /// modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public ValueSetResult SetValue(int value) {
            ValueSetResult result = _wrappedValue.SetValue(value);
            if(result == ValueSetResult.SUCCESS) {
                if(_changeCallback != null) 
                    _changeCallback.Invoke();
            }
            return result;
        }

        /// <summary>
        /// Sets this ConsoleVariable to the provided 
        /// integer / float / string / boolean.
        ///  
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, SetValue will do 
        /// its best to convert between types for you, but this 
        /// may result in bad data. 
        /// <para/> Returns TRUE if the ConsoleVariable was 
        /// modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public ValueSetResult SetValue(bool value) {
            ValueSetResult result = _wrappedValue.SetValue(value);
            if(result == ValueSetResult.SUCCESS) {
                if(_changeCallback != null) 
                    _changeCallback.Invoke();
            }
            return result;
        }

        /// <summary>
        /// Gets this ConsoleVariable's value as a string.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, Like SetValue(), GetString() 
        /// will do its best to convert between types for you.
        /// </summary>
        /// <returns></returns>
        public string GetString() {
            return _wrappedValue.GetString();
        }

        /// <summary>
        /// Gets this ConsoleVariable's value as a float.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, Like SetValue(), GetFloat() 
        /// will do its best to convert between types for you.
        /// </summary>
        /// <returns></returns>
        public float GetFloat() {
            return _wrappedValue.GetFloat();
        }

        /// <summary>
        /// Gets this ConsoleVariable's value as an integer.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, Like SetValue(), GetInt() 
        /// will do its best to convert between types for you.
        /// </summary>
        /// <returns></returns>
        public int GetInt() {
            return _wrappedValue.GetInt();
        }

        /// <summary>
        /// Gets this ConsoleVariable's value as a boolean.
        /// 
        /// <para/> Note:
        /// You must keep track of the backing type of this
        /// ConsoleVariable's Keyword Argument. No exceptions
        /// are thrown for mismatched types, Like SetValue(), GetBool() 
        /// will do its best to convert between types for you.
        /// </summary>
        /// <returns></returns>
        public bool GetBool() {
            return _wrappedValue.GetBool();
        }

        /// <summary>
        /// Resets this ConsoleVaraible to its default value.
        /// 
        /// Returns TRUE if the ConsoleVariable was modified as a result of this call.
        /// </summary>
        /// <returns></returns>
        public bool Reset() {
            return _wrappedValue.Reset();
        }

        public void OnValueChanged() {
            _wrappedValue.OnValueChanged();
        }

        /// <summary>
        /// Gets the type of this ConsoleVariable as a formatted string.
        /// Useful for printing in the console.
        /// </summary>
        /// <returns></returns>
        public string GetTypeName() {
            return _wrappedValue.GetTypeName();
        }

        public override string ToString() {
            return _wrappedValue.ToString();
        }
    }

    public class CVarRegistrySummary {
        private readonly List<ConsoleVariable> _vars = new();
        private readonly List<SaveableDataSet> _targetRefs = new();

        public void PushRegistrations() {
            for(int x = 0; x < _vars.Count; x++) {
                SaveableDataSet target = _targetRefs[x];
                ConsoleVariable convar = _vars[x];

                if(target == null) {
                    if(convar == null) {
                        Debug.LogWarning("A console variable was filled for this register but no data exists at this index. How did this happen? How did you do this? Why?");
                        continue;
                    }
                    Debug.LogWarning($@"ConsoleVariable {convar.GetID()} was found to have no serialization target. It will be corrected to 'SESSION.'");
                    INFRA.Game.Data.Session.AppendDataReference(convar);
                    continue;
                }

                SaveableDataSet previous = INFRA.Game.Data.GetDataSetContaining(convar);
                if(previous != null) {
                    if(previous.GetSaveableID().Equals(target.GetSaveableID())) 
                        Debug.LogWarning($@"Repeat registration of ConsoleVariable bound to ID '{convar.GetID()}' - This ID is taken!");
                    else Debug.LogWarning($@"Repeat registration of ConsoleVariable bound to ID '{convar.GetID()}' - This ID is already registered to '{previous.GetSaveableID()}'!");
                    continue;
                }

                target.AppendDataReference(convar);
                Debug.Log($@"Registered '{convar}' to serialization target '{target.GetSaveableID()}'");
            }

            _vars.Clear();
            _targetRefs.Clear();
        }

        public void Put(ConsoleVariable cvar, SaveableDataSet target) {
            _vars.Add(cvar);
            _targetRefs.Add(target);
        }
    }

    public class ConsoleVariableRegistrar : IKwArgBuilder<ConsoleVariableRegistrar> {

        private readonly CVarRegistrySummary _registry;
        internal Action _callback = null;

        public ConsoleVariableRegistrar(CVarRegistrySummary summary) {
            _registry = summary;
        }

        public ConsoleVariableRegistrar MakeKwArg(IKwArg arg, SaveableDataSet target) {
            if(target == null) {
                Debug.LogError($@"Error registering ConsoleVariable '{arg.GetID()}' - No 
                    valid SaveableDataSet was provided! This registration will be skipped.");
                return this;
            }
            _registry.Put(new ConsoleVariable(arg), target);
            return this;
        }

        public ConsoleVariableBasicBuilder<ConsoleVariableRegistrar> New(string name) {
            return new ConsoleVariableBasicBuilder<ConsoleVariableRegistrar>(this, name);
        }

        public class ConsoleVariableBasicBuilder<T> where T : IKwArgBuilder<T> {

            readonly T _root;
            readonly string _name;

            public SaveableDataSet _target = INFRA.Game.Data.GetDefault();

            internal ConsoleVariableBasicBuilder(T root, string name) {
                _root = root;
                _name = name;
            }

            public ConsoleVariableBasicBuilder<T> SavesTo(SaveableDataSet target) {
                if(target == null) _target = INFRA.Game.Data.GetDefault();
                else _target = target;
                return this;
            }

            public ConsoleVariableBasicBuilder<T> WithCallback(Action callback) {
                
                return this;
            }

            public KeywordArgumentBuilder<T> WithDescription(string desc) {
                return new KeywordArgumentBuilder<T>(_root, _name, desc, _target);
            }
        }
    }
}