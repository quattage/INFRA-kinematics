
// SIGNATURE :)

using System;
using System.IO;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data {

    /// <summary>
    /// A DataSource is a logical representation of a JSON file in the file system.
    /// Used for saving gamedata such as graphics settings, game saves, and physics stuff.
    /// </summary>
    public class DataSource {

        protected string _defpath;
        protected string _filename;
        public string FileName {  get => _filename; }

        public DataSource(string defpath, params string[] fileLocation) {
            _defpath = defpath;

            if(fileLocation == null || fileLocation.Length == 0)
                return;

            foreach(string sub in fileLocation)
                _defpath = Path.Combine(_defpath, sub);
            _defpath += ".json";
            _filename = Path.GetFileName(_defpath) + ".json";
        }
        
        public virtual bool Write(string fileContents) {
            try {
                if(!Directory.Exists(_defpath))
                    Directory.CreateDirectory(Path.GetDirectoryName(_defpath));
                if(File.Exists(_defpath)) File.Delete(_defpath);
                File.WriteAllText(_defpath, fileContents);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Error occured occured while writing to ({_defpath}) - Potential data corruption, Exception is as follows: {e}");
                return false;
            }
        }

        public virtual bool Read(out string result) {
            if(!IsPresent()) {
                result = "";
                return false;
            }
            try {
                result = File.ReadAllText(_defpath);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Unknown error occured while reading from '{_defpath}' - {e}");
                result = "";
                return false;
            }
        }

        public bool IsPresent() {
            return File.Exists(_defpath);
        }

        public virtual bool CanRead() {
            return true;
        }

        public virtual bool CanWrite() {
            return true;
        }

        public override string ToString() {
            return $@"DataSource ({_defpath})";
        }
    }

    /// <summary>
    /// A NullDataSource is just a DataSource that doesn't point to any file in particular.
    /// SaveData that contains a NullDataSource won't read or write.
    /// </summary>
    public class NullDataSource : DataSource {

        public NullDataSource() : base("", "") {  }

        public override bool Write(string fileContents) {
            return false;
        }

        public override bool Read(out string result) {
            result = "";
            return false;
        }

        public override bool CanRead() {
            return false;
        }

        public override bool CanWrite() {
            return true;
        }

        public override string ToString() {
            return "DataSource (No Associated File)";
        }
    }

    /// <summary>
    /// A MutableDataSource is a DataSource whose cooresponding file can change.
    /// Any file whose name/location can't be fixed/confirmed at compile time should
    /// use a MutableDataSource. (ex. a game save profile - where the user can rename it, make a new one, etc.)
    /// </summary>
    public class MutableDataSource : DataSource {

        private readonly string _prePath;

        public MutableDataSource(string defpath, params string[] fileLocation) : base(defpath, fileLocation) {
            _prePath = defpath;
        }

        public void SetFileName(string name) {
            string dir = Path.GetDirectoryName(_defpath);
            _defpath = Path.Combine(dir, name + ".json");
        }

        public void SetPath(params string[] fileLocation) {
            _defpath = _prePath;
            foreach(string sub in fileLocation)
                _defpath = Path.Combine(_defpath, sub);
        }

        public override string ToString() {
            return $@"MutableDataSource ({_defpath})";
        }
    }
}