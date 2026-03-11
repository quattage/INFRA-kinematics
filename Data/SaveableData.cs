
// SIGNATURE :)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.quatworks.INFRASEC.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data {

    /// <summary>
    /// A Saveable Data Set is a small collection of primitives
    /// (int, bool, string, enum, float) that are serialized to a JSON file.
    /// </summary>
    public class SaveableDataSet : SaveableData, IEnumerable<SaveableData> {
        
        private readonly string _refid;
        private readonly DataSource _source;

        private readonly Dictionary<string, SaveableData> _data = new();
        public IReadOnlyDictionary<string, SaveableData> Raw { get => _data; }

        public SaveableData this[string key] {
            get => GetDataReference(key);
        }

        public SaveableDataSet(string refid, DataSource file) {
            _refid = refid;
            _source = file;
        }

        public SaveableData SearchForToken(string name) {
            foreach(SaveableData data in _data.Values)
                if(name.Equals(data.GetSaveableID())) return data;
            return null;
        }

        public bool AppendDataReference(SaveableData element) {
            if(ContainsDataReference(element)) return false;
            _data.Add(element.GetSaveableID(), element);
            return true;
        }

        public bool ContainsDataReference(SaveableData element) {
            return _data.ContainsKey(element.GetSaveableID());
        }

        public bool ContainsDataReference(string key) {
            return _data.ContainsKey(key);
        }

        public SaveableData GetDataReference(string key) {
            if(_data.TryGetValue(key, out SaveableData data)) return data;
            return null;
        }

        public override string GetSaveableID() {
            return _refid;
        }

        /// <summary>
        /// Updates the serializable values contained within this 
        /// SaveableDataSet with the data contained within its 
        /// backing DataSource and cooresponding file on disk.
        /// Serializable values can be marked using Unity's 
        /// [SerializeField] attribute like normal. Classes that 
        /// don't inherit from MonoBehaviour must be marked as 
        /// [System.Serializable] to be read properly/ If a class 
        /// and its fields aren't properly marked in code, this 
        /// call simply won't do anything.
        /// </summary>
        /// <param name="source"></param>
        public virtual void ReadFromFile() {
            if(!_source.CanRead()) return;
            if(!_source.IsPresent()) {
                Debug.Log($@"Performing first-time write for container '{GetSaveableID()}'");
                string json = GetSerialized();
                _source.Write(json);
                _isUpToDate = true;
                return;
            }

            if(!_source.Read(out string file)) return;

            DeserializeFrom(file);
            if(NeedsSynced) {
                string json = GetSerialized();
                _source.Write(json);
                _isUpToDate = true;
                Debug.Log($@"Loaded container '{GetSaveableID()}' from {_source} with registry changes synced.");
            } else
                Debug.Log($@"Loaded container '{GetSaveableID()}' from {_source}.");
        }

        /// <summary>
        /// Serializes the data within this SaveableDataSet to a JSON string
        /// and overwrites the backing DataSource's file on disk with the 
        /// contents of  said string. ALl contents of the file at the time 
        /// of invocation will be replaced.
        /// </summary>
        /// <param name="source"></param>
        public virtual void WriteToFile() {
            if(!_source.CanWrite()) return;
            Debug.Log($@"Saving {GetSaveableID()}...");
            string json = GetSerialized();
            _source.Write(json);
            _isUpToDate = true;
        }

        public override SaveableData DeserializeFrom(string jsonString) {

            if(jsonString.IsNullOrEmpty()) return this;
            JArray data;
            _isUpToDate = true;

            try {
                JObject json = JObject.Parse(jsonString);
                data = (JArray)json[GetSaveableID()];
                if(data == null) {
                    Debug.LogError($@"Error Deserializing data from ({_source.FileName}) - Top-level construct '{GetSaveableID()}' couldn't be found in the target file!");
                    return this;
                }
            } catch(Exception ex) {
                Debug.LogError($@"Error Deserializing data from ({_source.FileName}) - Potential malformed JSON, Exception is as follows: {ex}");
                return this;
            }

            for(int x = 0; x < data.Count; x++) {
                JToken item = data[x];
                string objID = item["id"].ToString();
                string objJson = item["dat"].ToString();

                // if the json contains a token that isn't in the registry:
                if(!_data.TryGetValue(objID, out SaveableData lookup)) {
                    _isUpToDate = false; 
                    continue;
                }

                lookup.DeserializeFrom(objJson);

                // if the registry has a new entry that isn't in the json:
                if(lookup.NeedsSynced) _isUpToDate = false;
            }

            return this;
        }

        public override string GetSerialized() {
            StringBuilder json = new StringBuilder("{" + "\"" + GetSaveableID() + "\":[");
            int x = 0;
            foreach(SaveableData element in _data.Values) {

                x++;
                json.Append("{");
                json.Append($"\"id\":\"{element.GetSaveableID()}\",");
                json.Append($"\"dat\":{element.GetSerialized()}");
                json.Append("}");
                if(x < _data.Values.Count) json.Append(",");

            }
            json.Append("]}");
            return JToken.Parse(json.ToString()).ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _data.Values.GetEnumerator();
        }

        public IEnumerator<SaveableData> GetEnumerator() {
            return _data.Values.GetEnumerator();
        }

        public override bool Equals(object obj) {
            if(!(obj is SaveableDataSet other)) return false;
            return GetSaveableID().Equals(other.GetSaveableID());
        }   

        public override int GetHashCode() {
            return GetSaveableID().GetHashCode();
        }

        public override string ToString() {
            string output = $"Set '{GetSaveableID()}' - {Raw.Count} Members<br>";
            foreach(SaveableData entry in Raw.Values)
                output += "  <color=#9aacbc>•</color> " + entry.ToString() + "<br>";
            return output;
        }
    }

    /// <summary>
    /// Utility interface for writing to JSON
    /// You can use whatever you want to write/read to/from JSON,
    /// as long as what you do produces/consumes a valid JSON string
    /// </summary>}
    public abstract class SaveableData {

        // used to force rewrites if new registries exist
        protected bool _isUpToDate;
        public bool NeedsSynced { get { return !_isUpToDate; } }

        /// <summary>
        /// Loads data into this SaveableData object from the provided
        /// JSON string. If the provided string is empty, this call
        /// does nothing.<para/>
        /// Override to define custom deserialization behavior - 
        /// Calls will catch and consume their own exceptions.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual SaveableData DeserializeFrom(string jsonString) {
            _isUpToDate = false;
            if(jsonString.IsNullOrEmpty()) return this;
            try {
                JsonUtility.FromJsonOverwrite(jsonString, this);
                _isUpToDate = true; // up to date only if the registry was correctly deserialized
            } catch(Exception ex) {
                Debug.LogWarning($@"Error deserializing '{GetSaveableID()}' - Potential malformed JSON, Exception is as follows: {ex}");
            }
            return this;
        }

        /// <summary>
        /// Writes data from this SaveableData object to a 
        /// JSON string and returns it. <para/>
        /// Override to define custom serialization behavior.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerialized() {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Updates the serializable values contained within this 
        /// SaveableData object with the values contained in the 
        /// provided DataSource's file on disk. Serializable values
        /// can be marked using Unity's [SerializeField] attribute
        /// like normal. Classes that don't inherit from MonoBehaviour
        /// must be marked as [System.Serializable] to be read properly.
        /// If a class and it's fields aren't properly marked in code,
        /// this call simply won't do anything.
        /// </summary>
        /// <param name="source"></param>
        public virtual void ReadFromFile(DataSource source) {
            if(!source.Read(out string file)) return;
            DeserializeFrom(file);
        }

        /// <summary>
        /// Serializes the data within this SaveableData object to a JSON string
        /// and overwrites the given DataSource's file with the contents of 
        /// said string. ALl contents of the file at the time of invocation
        /// will be replaced.
        /// </summary>
        /// <param name="source"></param>
        public virtual void WriteToFile(DataSource target) {
            string json = GetSerialized();
            target.Write(json);
        }

        /// <summary>
        /// Returns a unique string identnfier for this SaveableData.
        /// </summary>
        /// <returns></returns>
        public abstract string GetSaveableID();

        public override bool Equals(object obj) {
            if(obj is SaveableData other)
                return GetSaveableID().Equals(other.GetSaveableID());
            return false;
        }

        public override int GetHashCode() {
            return GetSaveableID().GetHashCode();
        }
    }
}