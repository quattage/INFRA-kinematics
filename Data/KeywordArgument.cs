
// SIGNATURE :)

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data {

    public interface IKwArgBuilder<T> {
        public abstract T MakeKwArg(IKwArg arg, SaveableDataSet target);
    }

    public class KeywordArgumentBuilder<T> where T : IKwArgBuilder<T> {

        private readonly Regex nameFilter = new("[^a-z_]");
        private readonly Regex descFilter = new("[^a-zA-Z0-9_.!?: ]");
        internal readonly T _rootBuilder;

        internal readonly string _name;
        internal readonly string _description;
        internal readonly SaveableDataSet _serializeTarget;

        public KeywordArgumentBuilder(T rootBuilder, string name, string description) {
            _rootBuilder = rootBuilder;
            _name = nameFilter.Replace(name, "");
            if(!_name.Equals(name))
                Debug.LogWarning($@"KeywordArgument {name} contains invalid characters! Characters that don't match (^a-z_) characters will be stripped in the registered KeywordArgument instance.");
            _description = descFilter.Replace(description, "");
            _serializeTarget = null;
        }

        public KeywordArgumentBuilder(T rootBuilder, string name, string description, SaveableDataSet serializeTarget) {
            _rootBuilder = rootBuilder;
            _name = nameFilter.Replace(name, "");
            if(!_name.Equals(name))
                Debug.LogWarning($@"KeywordArgument {name} contains invalid characters! Characters that don't match (^a-z_) characters will be stripped in the registered KeywordArgument instance.");
            _description = descFilter.Replace(description, "");
            _serializeTarget = serializeTarget;
        }

        /// <summary>
        /// Constructs a Keyword Argument as a list of strings addressable
        /// by index.
        /// </summary>
        /// <returns></returns>
        public EnumKeywordArgument.Builder<T> AsEnum() {
            return new EnumKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as a singular string.
        /// </summary>
        /// <returns></returns>
        public StringKeywordArgument.Builder<T> AsString() {
            return new StringKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as a float value
        /// with an optional range.
        /// </summary>
        /// <returns></returns>
        public FloatKeywordArgument.Builder<T> AsFloat() {
            return new FloatKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as a float value
        /// with an optional range. A Backed float value is
        /// a float value that is already present within some
        /// pre-existing system, and doesn't need to be stored
        /// redundantly. Instead, the resulitng KeywordArgument
        /// will function as a wrapper for the supplied getter and
        /// setter.
        /// </summary>
        /// <returns></returns>
        public BackedFloatKeywordArgument.Builder<T> AsBackedFloat() {
            return new BackedFloatKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as an integer value
        /// with an optional range.
        /// </summary>
        /// <returns></returns>
        public IntKeywordArgument.Builder<T> AsInt() {
            return new IntKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as an integer value
        /// with an optional range. A Backed integer value is
        /// an integer value that is already present within some
        /// pre-existing system, and doesn't need to be stored
        /// redundantly. Instead, the resulitng KeywordArgument
        /// will function as a wrapper for the supplied getter and
        /// setter.
        /// </summary>
        /// <returns></returns>
        public BackedIntKeywordArgument.Builder<T> AsBackedInt() {
            return new BackedIntKeywordArgument.Builder<T>(this);
        }

        /// <summary>
        /// Constructs a Keyword Argument as a single boolean
        /// value.
        /// </summary>
        /// <returns></returns>
        public BoolKeywordArgument.Builder<T> AsBool() {
            return new BoolKeywordArgument.Builder<T>(this);
        }
    }

    public enum ValueSetResult {
        SUCCESS,
        BAD_RANGE,
        BAD_TYPE,
        PASS
    }

    /// <summary>
    /// The most basic requirements for a Keyword Argument wrapper.
    /// <para/> Important note: All non-abstract implementing subclasses 
    /// MUST be marked as serializable and have their value container 
    /// properly serialized. Serialization is required for use with 
    /// the Console system and won't throw errors if it isn't done 
    /// correctly.
    /// </summary>
    public interface IKwArg {

        public abstract string GetID();
        public abstract string GetDescription();

        public abstract IRangeable GetRange();

        public abstract ValueSetResult SetValue(string value);
        public abstract ValueSetResult SetValue(float value);
        public abstract ValueSetResult SetValue(int value);
        public abstract ValueSetResult SetValue(bool value);

        /// <summary>
        /// Gets the value associated with this IKwArg.
        /// Prone to null reference exceptions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string GetString();

        /// <summary>
        /// Gets the value associated with this IKwArg.
        /// Prone to null reference exceptions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract float GetFloat();

        /// <summary>
        /// Gets the value associated with this IKwArg.
        /// Prone to null reference exceptions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract int GetInt();

        /// <summary>
        /// Gets the value associated with this IKwArg.
        /// Prone to null reference exceptions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool GetBool();

        public string GetTypeName();

        /// <summary>
        /// Resets this KeywordArgument to its default.
        /// Returns true if this KeywordArgument was
        /// modified as a result of this call
        /// </summary>
        /// <returns></returns>
        public abstract bool Reset();

        /// <summary>
        /// Called wheneer SetValue() results in a successful
        /// value change.
        /// </summary>
        public abstract void OnValueChanged();
    }

    /// <summary>
    /// Used in place of nulls when searching
    /// </summary>
    public class InvalidKeywordArgument : IKwArg {
        readonly string _attemptID;
        public InvalidKeywordArgument(string attemptID) {
            _attemptID = attemptID;
        }

        

        public string GetDescription() {
            return $@"Registration didn't contain any data at '{_attemptID}'
                - Check logs for registration or retreival failures.";
        }

        public string GetID() {
            return _attemptID;
        }

        public string GetTypeName() {
            return "(Invalid)";
        }

        public string GetString() {
            Debug.LogWarning($@"Attempted to run GetString() on invalid kwarg 
                '{GetID()}' - Check logs for registration or retreival failures.");
            return "";
        }

        public float GetFloat() {
            Debug.LogWarning($@"Attempted to run GetFloat() on invalid kwarg 
                '{GetID()}' - Check logs for registration or retreival failures.");
            return 0;
        }

        public int GetInt() {
            Debug.LogWarning($@"Attempted to run GetInt() on invalid kwarg 
                '{GetID()}' - Check logs for registration or retreival failures.");
            return 0;
        }

        public bool GetBool() {
            Debug.LogWarning($@"Attempted to run GetBool() on invalid kwarg 
                '{GetID()}' - Check logs for registration or retreival failures.");
            return false;
        }

        public IRangeable GetRange() {
            return new ConstantRange();
        }

        public void OnValueChanged() {
            return;
        }

        public ValueSetResult SetValue(string value) {
            Debug.LogWarning($@"Attempted SetValue on invalid kwarg '{GetID()}' 
                - Check logs for registration or retreival failures.");
            return ValueSetResult.PASS;
        }

        public ValueSetResult SetValue(float value) {
            Debug.LogWarning($@"Attempted SetValue on invalid kwarg '{GetID()}' 
                - Check logs for registration or retreival failures.");
            return ValueSetResult.PASS;
        }

        public ValueSetResult SetValue(int value) {
            Debug.LogWarning($@"Attempted SetValue on invalid kwarg '{GetID()}' 
                - Check logs for registration or retreival failures.");
            return ValueSetResult.PASS;
        }

        public ValueSetResult SetValue(bool value) {
            Debug.LogWarning($@"Attempted SetValue on invalid kwarg '{GetID()}' 
                - Check logs for registration or retreival failures.");
            return ValueSetResult.PASS;
        }

        public bool Reset() {
            return false;
        }

        public override string ToString() {
            return $@"kwarg '{GetID()}' {GetTypeName()}";
        }
    }

    /// <summary>
    /// A KeywordArgument is a simple wrapper for an ingestible/serializable value
    /// that can be serialized and accessed to/from the in-game console via KeywordArguments
    /// or as arguments for commands. This value can be anything, and is not bound by generic type.
    /// </summary>
    public abstract class KeywordArgument : IKwArg {

        protected readonly string _id;
        protected readonly string _description;

        public KeywordArgument(string id, string description) {
            _id = id;
            _description = description;
        }

        public string GetID() {
            return _id;
        }

        public string GetDescription() {
            return _description;
        }

        public void OnValueChanged() {
            // TODO figure out whether or not callbacks need to happen
        }

        public abstract ValueSetResult SetValue(string value);
        public abstract ValueSetResult SetValue(float value);
        public abstract ValueSetResult SetValue(int value);
        public abstract ValueSetResult SetValue(bool value);
        
        public abstract string GetString();
        public abstract float GetFloat();
        public abstract int GetInt();
        public abstract bool GetBool();

        public abstract string GetTypeName();

        public abstract IRangeable GetRange();
        public abstract bool Reset();
    }

    /// <summary>
    /// A KeywordArgument that stores a list of strings. Can be addressed by
    /// string value or integer index.
    /// </summary>
    [System.Serializable]
    public class EnumKeywordArgument : KeywordArgument {

        [SerializeField] private int _index;
        private readonly EnumRange _range;
        private readonly int _defaultIndex;

        public EnumKeywordArgument(string name, string description, int defaultIndex, EnumRange range) : base(name, description) {
            _defaultIndex = defaultIndex;
            _index = defaultIndex;
            _range = range;
        }

        public override ValueSetResult SetValue(string value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            string old = GetString();
            if(old.Equals(value)) return ValueSetResult.PASS;
            _index = (int)_range.ParseValue(value);
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            int floored = (int)Mathf.Floor(value);
            if(_index == floored) return ValueSetResult.PASS;
            _index = floored;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(_index == value) return ValueSetResult.PASS;
            _index = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            return SetValue(value ? 1 : 0);
        }

        public override string GetString() {
            return _range.ParseValue(_index);
        }

        public override float GetFloat() {
            return _index;
        }

        public override int GetInt() {
            return _index;
        }

        public override bool GetBool() {
            return _index == _defaultIndex;
        }

        public override IRangeable GetRange() {
            return _range;
        }

        public override bool Reset() {
            if(_index.Equals(_defaultIndex)) return false;
            _index = _defaultIndex;
            return true;
        }

        public override string GetTypeName() {
            return "(Enum)";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#5f68db>{GetTypeName()}</color> <color=#a4b6b0>{GetRange()}</color>)";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {

            readonly KeywordArgumentBuilder<T> _prev;

            public Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }

            /// <summary>
            /// Adds a list of string values to the resulting EnumKeywordArgument
            /// in a similar way to declaring a standard Enum.
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public BuilderB WithValues(params string[] values) {
                return new BuilderB(this, values);
            }

            public class BuilderB {

                readonly Builder<T> _a;
                readonly string[] _values;

                internal BuilderB(Builder<T> a, params string[] values) {
                    _values = values;
                    _a = a;
                }

                /// <summary>
                /// Sets the default state for this EnumKeywordArgument.
                /// If the value isn't contained within the provided values
                /// in the previous step, this registration will fail.
                /// You're able to provide a string or an integer index.
                /// </summary>
                /// <param name="def"></param>
                /// <returns></returns>
                public BuilderC DefaultsTo(string def) {
                    return new BuilderC(def, this);
                }

                /// <summary>
                /// Sets the default state for this EnumKeywordArgument.
                /// The provided default value can either be a string state
                /// name or an integer index cooresponding to the previously
                /// defined values list. <para/>
                /// Note: If the value isn't contained within the provided values
                /// in the previous step, this registration will fail.
                /// </summary>
                /// <param name="def"></param>
                /// <returns></returns>
                public BuilderC DefaultsTo(int def) {
                    return new BuilderC(def, this);
                }

                public class BuilderC {
                    
                    readonly int? _defI;
                    readonly string _def;
                    readonly BuilderB _b;

                    internal BuilderC(string def, BuilderB b) {
                        _def = def;
                        _b = b;
                    }

                    internal BuilderC(int def, BuilderB b) {
                        _defI = def;
                        _b = b;
                    }

                    /// <summary>
                    /// Builds this KeywordArgument instance, adds it to the registry,
                    /// and returns the previous builder step for chaining.
                    /// </summary>
                    /// <returns></returns>
                    public T Make() {

                        if(_defI.HasValue) {
                            if(_defI < 0 || _defI >= _b._values.Length) {
                                Debug.LogError($@"Error creating EnumKeywordArgument '{_b._a._prev._name}' - The default value 
                                    at index {_defI} is out of range for [{_b._values}]. This registration will be skipped!");
                                return _b._a._prev._rootBuilder;
                            }
                            _b._a._prev._rootBuilder.MakeKwArg(new EnumKeywordArgument(_b._a._prev._name, _b._a._prev._description, _defI.Value, new EnumRange(_b._values)), _b._a._prev._serializeTarget);
                            return _b._a._prev._rootBuilder;
                        }

                        int ind = _b._values.IndexOf(_def);
                        if(ind <= -1) {
                            if(int.TryParse(_def, out ind)) {
                                _b._a._prev._rootBuilder.MakeKwArg(new EnumKeywordArgument(_b._a._prev._name, _b._a._prev._description, ind, new EnumRange(_b._values)), _b._a._prev._serializeTarget);
                                return _b._a._prev._rootBuilder;
                            }
                            Debug.LogError($@"Error creating EnumKeywordArgument '{_b._a._prev._name}' - The default keyword 
                                '{_def}' does not exist in {_b._values.ContentsToString()}. This registration will be skipped!");
                            return _b._a._prev._rootBuilder;
                        }

                        _b._a._prev._rootBuilder.MakeKwArg(new EnumKeywordArgument(_b._a._prev._name, _b._a._prev._description, ind, new EnumRange(_b._values)), _b._a._prev._serializeTarget);
                        return _b._a._prev._rootBuilder;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class StringKeywordArgument : KeywordArgument {

        [SerializeField] private string _value;
        private readonly string _default;

        public StringKeywordArgument(string name, string description, string defaultValue) : base(name, description) {
            _default = defaultValue;
            _value = _default;
        }

        public override ValueSetResult SetValue(string value) {
            if(value.Equals(_value)) return ValueSetResult.PASS;;
            _value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            string converted = value.ToString();
            if(converted.Equals(_value)) return ValueSetResult.PASS;
            _value = converted;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            string converted = value.ToString();
            if(converted.Equals(_value)) return ValueSetResult.PASS;
            _value = converted;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            string converted = value ? "true" : "false";
            if(converted.Equals(_value)) return ValueSetResult.PASS;
            _value = converted;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override string GetString() {
            return _value;
        }

        public override float GetFloat() {
            return _value.GetHashCode();
        }

        public override int GetInt() {
            return _value.GetHashCode();
        }

        public override bool GetBool() {
            if(_value.Equals("")) return false;
            return true;
        }

        public override IRangeable GetRange() {
            return null; // string values have no range
        }

        public override bool Reset() {
            if(_default == null) return false;
            if(_value.Equals(_default)) return false;
            _value = _default;
            return true;
        }

        public override string GetTypeName() {
            return "(String)";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#5fc9db>{GetTypeName()}</color> <color=#a4b6b0>any</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {

            readonly KeywordArgumentBuilder<T> _prev;

            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }

            public BuilderB DefaultsTo(string def) {
                return new BuilderB(this, def);
            }

            public class BuilderB {
                readonly Builder<T> _a;
                readonly string _def;
                internal BuilderB(Builder<T> a, string def) {
                    _a = a;
                    _def = def;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {
                    _a._prev._rootBuilder.MakeKwArg(new StringKeywordArgument(_a._prev._name, _a._prev._description, _def), _a._prev._serializeTarget);
                    return _a._prev._rootBuilder;
                }
            }
        }
    }

    [System.Serializable]
    public class FloatKeywordArgument : KeywordArgument {

        [SerializeField] private float _value;
        private readonly IRangeable _range;
        private readonly float? _default;

        public FloatKeywordArgument(string name, string description, float defaultValue, IRangeable range) : base(name, description) {
            _default = defaultValue;
            _value = defaultValue;
            _range = range;
        }

        public override ValueSetResult SetValue(string value) {
            if(!float.TryParse(value, out float parsed)) {
                Debug.LogWarning($@"Error setting value of KeywordArgument '{_id}' - 
                    The string value '{value}' couldn't be parsed to a float!");
                return ValueSetResult.BAD_TYPE;
            }
            
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(_value, parsed)) return ValueSetResult.PASS;
            _value = parsed;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(_value, value)) return ValueSetResult.PASS;
            _value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(_value, value)) return ValueSetResult.PASS;
            _value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            return SetValue(value ? 1f : 0f);
        }

        public override string GetString() {
            return _value.ToString();
        }

        public override float GetFloat() {
            return _value;
        }

        public override int GetInt() {
            return (int)Mathf.Floor(_value);
        }

        public override bool GetBool() {
            return !Mathf.Approximately(_value, 0);
        }

        public override IRangeable GetRange() {
            return _range;
        }

        public override bool Reset() {
            if(_value == _default.Value) return false;
            if(Mathf.Approximately(_value, _default.Value)) return false;
            _value = _default.Value;
            return true;
        }

        public override string GetTypeName() {
            return "(Float)";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#dbb55f>{GetTypeName()}</color> <color=#a4b6b0>{GetRange()}</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {
            readonly KeywordArgumentBuilder<T> _prev;
            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }
            public BuilderB DefaultsTo(float def) {
                return new BuilderB(this, def);
            }
            
            public class BuilderB {
                readonly Builder<T> _a;
                readonly float _def;
                internal BuilderB(Builder<T> a, float def) {
                    _a = a;
                    _def = def;
                }

                float? _min = null;
                float? _max = null;
                bool _canChange = true;

                /// <summary>
                /// Sets the minumum possible value for this float
                /// </summary>
                /// <param name="minimum"></param>
                /// <returns></returns>
                public BuilderB WithMinimum(float minimum) {
                    _min = minimum;
                    return this;
                }

                /// <summary>
                /// Sets the maximum possible value for this float
                /// </summary>
                /// <param name="maximum"></param>
                /// <returns></returns>
                public BuilderB WithMaximum(float maximum) {
                    _max = maximum;
                    return this;
                }

                /// <summary>
                /// Marks this float as immutable.
                /// </summary>
                /// <returns></returns>
                public BuilderB AsConstant() {
                    _canChange = false;
                    return this;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {

                    if(_canChange == false) {
                        _a._prev._rootBuilder.MakeKwArg(new FloatKeywordArgument(_a._prev._name, _a._prev._description, _def, new ConstantRange()), _a._prev._serializeTarget);
                        return _a._prev._rootBuilder;
                    }

                    IRangeable range = IRangeable.MakeNumericRange(_min, _max);
                    if(!range.Contains(_def)) {
                        Debug.LogError($@"Error creating FloatKeywordArgument '{_a._prev._name}'
                            - The default value {_def} does not exist in {range}");
                        return _a._prev._rootBuilder;
                    }

                    _a._prev._rootBuilder.MakeKwArg(new FloatKeywordArgument(_a._prev._name, _a._prev._description, _def, IRangeable.MakeNumericRange(_min, _max)), _a._prev._serializeTarget);
                    return _a._prev._rootBuilder;
                }
            }
        }
    }

    public class BackedFloatKeywordArgument : KeywordArgument, ISerializationCallbackReceiver {

        private Func<float> _getter;
        private Action<float> _setter;

        private float Value { get => _getter.Invoke(); set => _setter.Invoke(value); }
        [SerializeField] private float _backed;
        public void OnBeforeSerialize() { _backed = Value; }
        public void OnAfterDeserialize() { Value = _backed; }

        private readonly IRangeable _range;
        private readonly float? _default;

        private readonly string _backingDescription;

        public BackedFloatKeywordArgument(string name, string description, string backingDescription, float defaultValue, Func<float> wrappedGet, Action<float> wrappedSet, IRangeable range) : base(name, description) {
            _default = defaultValue;
            _setter = wrappedSet;
            _getter = wrappedGet;
            _range = range;
            _backingDescription = backingDescription;
        }

        public override ValueSetResult SetValue(string value) {
            if(!float.TryParse(value, out float parsed)) {
                Debug.LogWarning($@"Error setting value of KeywordArgument '{_id}' - 
                    The string value '{value}' couldn't be parsed to a float!");
                return ValueSetResult.BAD_TYPE;
            }
            
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(Value, parsed)) return ValueSetResult.PASS;
            Value = parsed;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(Value, value)) return ValueSetResult.PASS;
            Value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Mathf.Approximately(Value, value)) return ValueSetResult.PASS;
            Value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            return SetValue(value ? 1f : 0f);
        }

        public override string GetString() {
            return Value.ToString();
        }

        public override float GetFloat() {
            return Value;
        }

        public override int GetInt() {
            return (int)Mathf.Floor(Value);
        }

        public override bool GetBool() {
            return !Mathf.Approximately(Value, 0);
        }

        public override IRangeable GetRange() {
            return _range;
        }

        public override bool Reset() {
            float currentValue = Value;
            if(currentValue == _default.Value) return false;
            if(Mathf.Approximately(currentValue, _default.Value)) return false;
            Value = _default.Value;
            return true;
        }

        public override string GetTypeName() {
            return $"({_backingDescription})";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#dbb55f>{GetTypeName()}</color> <color=#a4b6b0>{GetRange()}</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {
            readonly KeywordArgumentBuilder<T> _prev;
            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }
            public BuilderB DefaultsTo(float def) {
                return new BuilderB(this, def);
            }

            public class BuilderB {
                internal readonly Builder<T> _a;
                internal readonly float _def;

                internal BuilderB(Builder<T> a, float def) {
                    _a = a;
                    _def = def;
                }

                public BuilderD WithSetter(Action<float> setter) {
                    return new BuilderD(this, setter);
                }
            }

            public class BuilderD {     //lol
                internal readonly BuilderB _b;
                internal readonly Action<float> _setter;
                public BuilderD(BuilderB b, Action<float> setter) {
                    _b = b;
                    _setter = setter;
                }

                public BuilderC WithGetter(Func<float> getter) {
                    return new BuilderC(this, getter);
                }
            }

            public class BuilderC {
                internal readonly BuilderD _d;
                internal readonly Func<float> _getter;
                internal BuilderC(BuilderD d, Func<float> getter) {
                    _d = d;
                    _getter = getter;
                }

                float? _min = null;
                float? _max = null;
                bool _canChange = true;

                /// <summary>
                /// Sets the minumum possible value for this float
                /// </summary>
                /// <param name="minimum"></param>
                /// <returns></returns>
                public BuilderC WithMinimum(float minimum) {
                    _min = minimum;
                    return this;
                }

                /// <summary>
                /// Sets the maximum possible value for this float
                /// </summary>
                /// <param name="maximum"></param>
                /// <returns></returns>
                public BuilderC WithMaximum(float maximum) {
                    _max = maximum;
                    return this;
                }

                /// <summary>
                /// Marks this float as immutable.
                /// </summary>
                /// <returns></returns>
                public BuilderC AsConstant() {
                    _canChange = false;
                    return this;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {

                    string mainDesc = _d._b._a._prev._description;
                    string extraDesc = "";

                    if(mainDesc.Contains("::")) {
                        string[] splitDesc = _d._b._a._prev._description.Split("::");
                        mainDesc = splitDesc[0];
                        extraDesc = splitDesc[1];
                    }

                    if(_canChange == false) {
                        _d._b._a._prev._rootBuilder.MakeKwArg(new BackedFloatKeywordArgument(_d._b._a._prev._name, mainDesc, extraDesc, _d._b._def, _getter, _d._setter, new ConstantRange()), _d._b._a._prev._serializeTarget);
                        return _d._b._a._prev._rootBuilder;
                    }

                    IRangeable range = IRangeable.MakeNumericRange(_min, _max);
                    if(!range.Contains(_d._b._def)) {
                        Debug.LogError($@"Error creating BackedFloatKeywordArgument '{_d._b._a._prev._name}'
                            - The default value {_d._b._def} does not exist in {range}");
                        return _d._b._a._prev._rootBuilder;
                    }

                    _d._b._a._prev._rootBuilder.MakeKwArg(new BackedFloatKeywordArgument(_d._b._a._prev._name, mainDesc, extraDesc, _d._b._def, _getter, _d._setter, IRangeable.MakeNumericRange(_min, _max)), _d._b._a._prev._serializeTarget);
                    return _d._b._a._prev._rootBuilder;
                }
            }
        }
    }

    [System.Serializable]
    public class IntKeywordArgument : KeywordArgument {

        [SerializeField] private int _value;
        private readonly IRangeable _range;
        private readonly int? _default;

        public IntKeywordArgument(string name, string description, int defaultValue, IRangeable range) : base(name, description) {
            _default = defaultValue;
            _value = defaultValue;
            _range = range;
        }

        public override ValueSetResult SetValue(string value) {
            int parsed;
            if(!int.TryParse(value, out parsed)) {
                if(float.TryParse(value, out float parsedFloat))
                    parsed = (int)Mathf.Floor(parsedFloat);
                else return ValueSetResult.BAD_TYPE;
            }
            if(!_range.Contains(value))  return ValueSetResult.BAD_RANGE;
            if(_value == parsed) return ValueSetResult.PASS;
            _value = parsed;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            int floored = (int)Mathf.Floor(value);
            if(value == floored) return ValueSetResult.PASS;
            _value = floored;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(_value == value) return ValueSetResult.PASS;
            _value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            return SetValue(value ? 1 : 0);
        }

        public override string GetString() {
            return _value.ToString();
        }

        public override float GetFloat() {
            return _value;
        }

        public override int GetInt() {
            return (int)Mathf.Floor(_value);
        }

        public override bool GetBool() {
            return _value > 0 ? true : false;
        }

        public override IRangeable GetRange() {
            return _range;
        }

        public override bool Reset() {
            if(!_default.HasValue) return false;
            if(Mathf.Approximately(_value, _default.Value)) return false;
            _value = _default.Value;
            return true;
        }

        public override string GetTypeName() {
            return "(Int)";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#86db5f>{GetTypeName()}</color> <color=#a4b6b0>{GetRange()}</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {
            readonly KeywordArgumentBuilder<T> _prev;
            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }
            public BuilderB DefaultsTo(int def) {
                return new BuilderB(this, def);
            }
            
            public class BuilderB {
                readonly Builder<T> _a;
                readonly int _def;
                internal BuilderB(Builder<T> a, int def) {
                    _a = a;
                    _def = def;
                }

                int? _min = null;
                int? _max = null;
                bool _canChange = true;

                public BuilderB WithMinimum(int minimum) {
                    _min = minimum;
                    return this;
                }

                public BuilderB WithMaximum(int maximum) {
                    _max = maximum;
                    return this;
                }

                public BuilderB AsConstant() {
                    _canChange = false;
                    return this;
                }

                public BuilderB WithCallback() {
                    return this;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {

                    if(_canChange == false) {
                        _a._prev._rootBuilder.MakeKwArg(new IntKeywordArgument(_a._prev._name, _a._prev._description, _def, new ConstantRange()), _a._prev._serializeTarget);
                        return _a._prev._rootBuilder;
                    }

                    IRangeable range = IRangeable.MakeNumericRange(_min, _max);
                    if(!range.Contains(_def)) {
                        Debug.LogError($@"Error creating FloatKeywordArgument '{_a._prev._name}'
                            - The default value {_def} does not exist in {range}");
                        return _a._prev._rootBuilder;
                    }

                    _a._prev._rootBuilder.MakeKwArg(new IntKeywordArgument(_a._prev._name, _a._prev._description, _def, IRangeable.MakeNumericRange(_min, _max)), _a._prev._serializeTarget);
                    return _a._prev._rootBuilder;
                }
            }
        }
    }

    [System.Serializable]
    public class BackedIntKeywordArgument : KeywordArgument, ISerializationCallbackReceiver {

        private Func<int> _getter;
        private Action<int> _setter;

        private int Value { get => _getter.Invoke(); set => _setter.Invoke(value); }
        [SerializeField] private int _backed = 0;
        public void OnBeforeSerialize() { _backed = Value; }
        public void OnAfterDeserialize() { Value = _backed; }

        private readonly IRangeable _range;
        private readonly int? _default;

        private readonly string _backingDescription;

        public BackedIntKeywordArgument(string name, string description, string backingDescription, int defaultValue, Func<int> wrappedGet, Action<int> wrappedSet, IRangeable range) : base(name, description) {
            _default = defaultValue;
            _setter = wrappedSet;
            _getter = wrappedGet;
            _range = range;
            _backingDescription = backingDescription;
        }

        public override ValueSetResult SetValue(string value) {
            if(!int.TryParse(value, out int parsed)) {
                Debug.LogWarning($@"Error setting value of KeywordArgument '{_id}' - 
                    The string value '{value}' couldn't be parsed to an int!");
                return ValueSetResult.BAD_TYPE;
            }
            
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Value == parsed) return ValueSetResult.PASS;
            Value = parsed;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            int floored = (int)Mathf.Floor(value);
            if(!_range.Contains(floored)) return ValueSetResult.BAD_RANGE;
            if(Value == floored) return ValueSetResult.PASS;
            Value = floored;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(int value) {
            if(!_range.Contains(value)) return ValueSetResult.BAD_RANGE;
            if(Value == value) return ValueSetResult.PASS;
            Value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(bool value) {
            return SetValue(value ? 1f : 0f);
        }

        public override string GetString() {
            return Value.ToString();
        }

        public override float GetFloat() {
            return Value;
        }

        public override int GetInt() {
            return (int)Mathf.Floor(Value);
        }

        public override bool GetBool() {
            return !Mathf.Approximately(Value, 0);
        }

        public override IRangeable GetRange() {
            return _range;
        }

        public override bool Reset() {
            float currentValue = Value;
            if(currentValue == _default.Value) return false;
            Value = _default.Value;
            return true;
        }

        public override string GetTypeName() {
            return $"({_backingDescription})";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#dbb55f>{GetTypeName()}</color> <color=#a4b6b0>{GetRange()}</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {
            readonly KeywordArgumentBuilder<T> _prev;
            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }
            public BuilderB DefaultsTo(int def) {
                return new BuilderB(this, def);
            }

            public class BuilderB {
                internal readonly Builder<T> _a;
                internal readonly int _def;

                internal BuilderB(Builder<T> a, int def) {
                    _a = a;
                    _def = def;
                }

                public BuilderD WithSetter(Action<int> setter) {
                    return new BuilderD(this, setter);
                }
            }

            public class BuilderD {     //lol
                internal readonly BuilderB _b;
                internal readonly Action<int> _setter;
                public BuilderD(BuilderB b, Action<int> setter) {
                    _b = b;
                    _setter = setter;
                }

                public BuilderC WithGetter(Func<int> getter) {
                    return new BuilderC(this, getter);
                }
            }

            public class BuilderC {
                internal readonly BuilderD _d;
                internal readonly Func<int> _getter;
                internal BuilderC(BuilderD d, Func<int> getter) {
                    _d = d;
                    _getter = getter;
                }

                int? _min = null;
                int? _max = null;
                bool _canChange = true;

                /// <summary>
                /// Sets the minumum possible value for this int
                /// </summary>
                /// <param name="minimum"></param>
                /// <returns></returns>
                public BuilderC WithMinimum(int minimum) {
                    _min = minimum;
                    return this;
                }

                /// <summary>
                /// Sets the maximum possible value for this int
                /// </summary>
                /// <param name="maximum"></param>
                /// <returns></returns>
                public BuilderC WithMaximum(int maximum) {
                    _max = maximum;
                    return this;
                }

                /// <summary>
                /// Marks this int as immutable.
                /// </summary>
                /// <returns></returns>
                public BuilderC AsConstant() {
                    _canChange = false;
                    return this;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {

                    string mainDesc = _d._b._a._prev._description;
                    string extraDesc = "";

                    if(mainDesc.Contains("::")) {
                        string[] splitDesc = _d._b._a._prev._description.Split("::");
                        mainDesc = splitDesc[0];
                        extraDesc = splitDesc[1];
                    }

                    if(_canChange == false) {
                        _d._b._a._prev._rootBuilder.MakeKwArg(new BackedIntKeywordArgument(_d._b._a._prev._name, mainDesc, extraDesc, _d._b._def, _getter, _d._setter, new ConstantRange()), _d._b._a._prev._serializeTarget);
                        return _d._b._a._prev._rootBuilder;
                    }

                    IRangeable range = IRangeable.MakeNumericRange(_min, _max);
                    if(!range.Contains(_d._b._def)) {
                        Debug.LogError($@"Error creating BackedIntKeywordArgument '{_d._b._a._prev._name}'
                            - The default value {_d._b._def} does not exist in {range}");
                        return _d._b._a._prev._rootBuilder;
                    }

                    _d._b._a._prev._rootBuilder.MakeKwArg(new BackedIntKeywordArgument(_d._b._a._prev._name, mainDesc, extraDesc, _d._b._def, _getter, _d._setter, IRangeable.MakeNumericRange(_min, _max)), _d._b._a._prev._serializeTarget);
                    return _d._b._a._prev._rootBuilder;
                }
            }
        }
    }


    [System.Serializable]
    public class BoolKeywordArgument : KeywordArgument {

        [SerializeField] private bool _value;
        private readonly bool? _default;

        public BoolKeywordArgument(string name, string description, bool defaultValue) : base(name, description) {
            _default = defaultValue;
            _value = defaultValue;
        }

        public override ValueSetResult SetValue(string value) {
            if(!bool.TryParse(value, out bool parsed)) 
                return ValueSetResult.BAD_TYPE;
            if(_value == parsed) return ValueSetResult.PASS;
            _value = parsed;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override ValueSetResult SetValue(float value) {
            return ValueSetResult.BAD_TYPE;
        }

        public override ValueSetResult SetValue(int value) {
            if(value == 0) return SetValue(false);
            if(value == 1) return SetValue(true);
            return ValueSetResult.BAD_TYPE;
        }

        public override ValueSetResult SetValue(bool value) {
            if(_value == value) return ValueSetResult.PASS;
            _value = value;
            OnValueChanged();
            return ValueSetResult.SUCCESS;
        }

        public override string GetString() {
            return _value ? "true" : "false";
        }

        public override float GetFloat() {
            return _value ? 1f : 0f;
        }

        public override int GetInt() {
            return _value ? 1 : 0;
        }

        public override bool GetBool() {
            return _value;
        }

        public override IRangeable GetRange() {
            return null; // bool values' range is implied to be [0, 1];
        }

        public override bool Reset() {
            if(!_default.HasValue) return false;
            if(_value == _default.Value) return false;
            _value = _default.Value;
            return true;
        }

        public override string GetTypeName() {
            return "(Bool)";
        }

        public override string ToString() {
            return $@"<size=15><b><color=#9aacbc>'{GetID()}'</color></b></size> <color=#db5fc1>{GetTypeName()}</color> <color=#a4b6b0>true / false</color>";
        }

        public class Builder<T> where T: IKwArgBuilder<T> {
            readonly KeywordArgumentBuilder<T> _prev;
            internal Builder(KeywordArgumentBuilder<T> prev) {
                _prev = prev;
            }
            public BuilderB DefaultsTo(bool def) {
                return new BuilderB(this, def);
            }
            
            public class BuilderB {
                readonly Builder<T> _a;
                readonly bool _def;
                internal BuilderB(Builder<T> a, bool def) {
                    _a = a;
                    _def = def;
                }

                /// <summary>
                /// Builds this KeywordArgument instance, adds it to the registry,
                /// and returns the previous builder step for chaining.
                /// </summary>
                /// <returns></returns>
                public T Make() {
                    _a._prev._rootBuilder.MakeKwArg(new BoolKeywordArgument(_a._prev._name, _a._prev._description, _def), _a._prev._serializeTarget);
                    return _a._prev._rootBuilder;
                }
            }
        }
    }


    /// <summary>
    /// A template class for defining an arbitrary range of values
    /// </summary>
    public interface IRangeable {
        /// <summary>
        /// Returns TRUE if the input value is within the range defined by this range object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool Contains(float value);

        /// <summary>
        /// Returns TRUE if the input value is within the range defined by this range object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool Contains(string value);

        /// <summary>
        /// Returns the input value clamped to the range defined by this range object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract float Clamp(float value);

        /// <summary>
        /// Returns the relevent value within this range mapped to the provided value.
        /// addressable states.
        /// </summary>
        /// <returns></returns>
        public abstract string ParseValue(float value);

        /// <summary>
        /// Returns the relevent value within this range mapped to the provided value.
        /// addressable states.
        /// </summary>
        /// <returns></returns>
        public abstract float ParseValue(string value);

        #nullable enable
        public static IRangeable MakeNumericRange(int? min, int? max)  {
            if(min.HasValue && max.HasValue) {
                float minR = min.Value;
                float maxR = max.Value;
                if(minR > maxR) {
                    maxR = minR;
                    minR = max.Value;
                }
                return new DoubleEndedRange(minR, maxR);
            }

            if(min.HasValue && !max.HasValue) {
                return new MinimumOpenRange(min.Value);
            }

            if(!min.HasValue && max.HasValue) {
                return new MaximumOpenRange(max.Value);
            }

            return new InfiniteRange();
        }

        public static IRangeable MakeNumericRange(float? min, float? max)  {
            if(min.HasValue && max.HasValue) {
                float minR = min.Value;
                float maxR = max.Value;
                if(minR > maxR) {
                    maxR = minR;
                    minR = max.Value;
                }
                return new DoubleEndedRange(minR, maxR);
            }

            if(min.HasValue && !max.HasValue) {
                return new MinimumOpenRange(min.Value);
            }

            if(!min.HasValue && max.HasValue) {
                return new MaximumOpenRange(max.Value);
            }

            return new InfiniteRange();
        }
        #nullable disable
    }

    public class DoubleEndedRange : IRangeable {

        readonly float _min;
        readonly float _max;

        public DoubleEndedRange(float min, float max) {
            _min = min;
            _max = max;
        }

        public float Clamp(float value) {
            return Mathf.Clamp(value, _min, _max);
        }

        public bool Contains(float value) {
            return value >= _min && value <= _max;
        }

        public bool Contains(string value) {
            float valueParsed = float.Parse(value);
            return valueParsed >= _min && valueParsed <= _max;
        }

        public bool IsAddressable() {
            return false;
        }

        public string ParseValue(float value) {
            return Clamp(value).ToString();
        }

        public float ParseValue(string value) {
            if(!float.TryParse(value, out float parsed)) return 0f;
            return Clamp(parsed);
        }

        public override string ToString() {
            return $@"from {_min} to {_max}";
        }
    }

    public class MinimumOpenRange : IRangeable {

        readonly float _min;

        public MinimumOpenRange(float min) {
            _min = min;
        }

        public float Clamp(float value) {
            return value >= _min ? value : _min;
        }

        public bool Contains(float value) {
            return value >= _min;
        }

        public bool Contains(string value) {
            return float.Parse(value) >= _min;
        }

        public string ParseValue(float value) {
            return Clamp(value).ToString();
        }

        public float ParseValue(string value) {
            if(!float.TryParse(value, out float parsed)) return 0f;
            return Clamp(parsed);
        }

        public override string ToString() {
            return $@">= {_min}";
        }
    }

    public class MaximumOpenRange : IRangeable {

        readonly float _max;

        public MaximumOpenRange(float max) {
            _max = max;
        }

        public float Clamp(float value) {
            return value <= _max ? value : _max;
        }

        public bool Contains(float value) {
            return value <= _max;
        }

        public bool Contains(string value) {
            return float.Parse(value) <= _max;
        }

        public string ParseValue(float value) {
            return Clamp(value).ToString();
        }

        public float ParseValue(string value) {
            if(!float.TryParse(value, out float parsed)) return 0f;
            return Clamp(parsed);
        }

        public override string ToString() {
            return $@"<= {_max}";
        }
    }


    /// <summary>
    /// A Range for storing an array of addressable states,
    /// similar to an Enum.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumRange : IRangeable {

        readonly string[] _states;

        public EnumRange(params string[] states) {
            _states = states;
        }

        public float Clamp(float value) {
            return Mathf.Clamp(value, 0, _states.Length - 1);
        }

        public bool Contains(float value) {
            return value >= 0 && value < _states.Length;
        }

        public bool Contains(string value) {
            return _states.Contains(value);
        }

        public string ParseValue(float value) {
            return _states[(int)Mathf.Floor(Clamp(value))];
        }

        public float ParseValue(string value) {
            for(int x = 0; x < _states.Length; x++)
                if(_states[x].Equals(value)) return x;
            return -1;
        }

        public override string ToString() {
            return _states.ContentsToString();
        }
    }

    /// <summary>
    /// A Range that doesn't impose any restrictions.
    /// Its Contains() method always returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InfiniteRange : IRangeable {
        public float Clamp(float value) {        
            return value;
        }

        public bool Contains(float value) {
            return true;
        }

        public bool Contains(string value) {
            return true;
        }

        public bool IsAddressable() {
            return false;
        }

        public string ParseValue(float value) {
            return value.ToString();
        }

        public float ParseValue(string value) {
            if(!float.TryParse(value, out float parsed)) return 0;
            return parsed;
        }

        public override string ToString() {
            return "Any";
        }
    }

    /// <summary>
    /// A Range that represents nothing. Its value
    /// cannot be changed. Its Contains() method 
    /// always returns false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConstantRange : IRangeable {

        public float Clamp(float value) {        
            return 0;
        }

        public bool Contains(float value) {
            return false;
        }

        public bool Contains(string value) {
            return false;
        }

        public bool IsAddressable() {
            return false;
        }

        public string ParseValue(float value) {
            return "";
        }

        public float ParseValue(string value) {
            return 0;
        }

        public override string ToString() {
            return "None (Constant)";
        }
    }
}