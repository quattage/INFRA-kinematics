
// SIGNATURE :)

using System.Collections.Generic;
using Assets.quatworks.INFRASEC.Data.Console;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data {

    public class DataContainer {

        /// <summary>
        /// SESSION variables aren't saved.
        /// </summary>
        public SaveableDataSet Session { get { return _sdSession; } }
        public IReadOnlyDictionary<string, SaveableData> SessionRaw { get { return _sdSession.Raw; } }
        private readonly SaveableDataSet _sdSession = new("session", new NullDataSource());

        /// <summary>
        /// The Client data store contains client varaibles like graphics settings, inputs, etc.
        /// </summary>
        public SaveableDataSet Client { get { return _sdClient; } }
        public IReadOnlyDictionary<string, SaveableData> ClientRaw { get { return _sdClient.Raw; } }
        private readonly SaveableDataSet _sdClient = new("client", new DataSource(Application.persistentDataPath, "client"));

        /// <summary>
        /// Server data store contains server-sided variables like physics tickrate, collision settings, etc.
        /// </summary>
        public SaveableDataSet Server { get { return _sdServer; } }
        public IReadOnlyDictionary<string, SaveableData> ServerRaw { get { return _sdServer.Raw; } }
        private readonly SaveableDataSet _sdServer = new("server", new DataSource(Application.persistentDataPath, "server"));

        /// <summary>
        /// Commands are stored in a tokenized dictionary tree that can be accessed here.
        /// </summary>
        /// <value></value>
        public BranchingCommandSet ConsoleCommands { get => _commands; }
        private readonly BranchingCommandSet _commands = new("Commands");
        

        public SaveableDataSet GetDefault() {
            return _sdSession;
        }

        public bool IsAccessRestricted(string err) {
            if(INFRA.Game.Stage == INFRA.GameLifecycle.LOADING) {
                Debug.LogError(err + " - Registry accessed outside of permissable loading context!");
                return true;
            }
            return false;
        }

        public void ForceWriteAll() {
            // since session stuff doesn't save, this call doesn't do anything - it's here for posterity :)
            Session.WriteToFile();
            Client.WriteToFile();
            Server.WriteToFile();
        }

        /// <summary>
        /// Updates all serializable data in this container's data sets with
        /// the stuff present on disk. <para/> Note: If this container's
        /// DataSource directory or file doesn't exist, this method
        /// will actually create the directory and write the file instead.
        /// </summary>
        public void ReadFromDisk() {
            // since session stuff doesn't save, this call doesn't do anything - it's here for posterity :)
            Session.ReadFromFile(); 

            Client.ReadFromFile();
            Server.ReadFromFile();
        }

        public bool ContainsRegistration(SaveableData data) {
            return GetDataSetContaining(data) != null;
        }

        public SaveableDataSet GetDataSetContaining(SaveableData data) {
            if(_sdSession.ContainsDataReference(data)) return _sdSession;
            if(_sdClient.ContainsDataReference(data)) return _sdClient;
            if(_sdServer.ContainsDataReference(data)) return _sdServer;
            return null;
        }

        // operator prioritizes server lookups for speed
        // TODO generic way of doing this that doesn't require bespoke implementation
        public SaveableData this[string key] {
            get {
                SaveableData tryGet = Server[key];
                if(tryGet != null) return tryGet;
                tryGet = Client[key];
                if(tryGet != null) return tryGet;
                tryGet = Session[key];
                return tryGet;
            }
        }

        public string GetCVarsAsString() {
            string output = "";
            output += _sdSession.ToString() + "<br><br>";
            output += _sdClient.ToString() + "<br><br>";
            output += _sdServer.ToString() + "<br><br>";
            return output;
        }

        public IKwArg SearchForArgument(string name) {
            SaveableData output;
            output = Session.SearchForToken(name);
            if(output is IKwArg arg0) return arg0;

            output = Client.SearchForToken(name);
            if(output is IKwArg arg1) return arg1;

            output = Server.SearchForToken(name);
            if(output is IKwArg arg2) return arg2;
            return null;
        }

        public int ResetAllArguments() {

            int output = 0;

            foreach(SaveableData data in _sdSession) {
                if(data is IKwArg arg) {
                    string old = arg.GetString();
                    arg.Reset();
                    if(!old.Equals(arg.GetString())) output++;
                }
            }

            foreach(SaveableData data in _sdClient) {
                if(data is IKwArg arg) {
                    string old = arg.GetString();
                    arg.Reset();
                    if(!old.Equals(arg.GetString())) output++;
                }
            }

            foreach(SaveableData data in _sdServer) {
                if(data is IKwArg arg) {
                    string old = arg.GetString();
                    arg.Reset();
                    if(!old.Equals(arg.GetString())) output++;
                }
            }
            
            return output;
        }

        public string GetCommandsAsString() {
            return "<br>" + _commands.GetHelpString();
        }
    }
}