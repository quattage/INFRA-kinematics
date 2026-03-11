
// SIGNATURE :)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data.Console{


    public class ConsoleCommandRegistrar {

        internal static readonly Regex CMD_FILTER = new("[^a-z0-9_]");

        internal readonly Dictionary<string, IConsoleRegistryBranchable> _commandsToAdd = new();

        public ConsoleCommandPathAndArgumentBuilder New(Type type, string path) {

            // Filter the path for bad symbols and whitespace
            List<string> splitPath = path.Split(" ").ToList();
            int removedTokens = 0;
            for(int x = splitPath.Count - 1; x >= 0; x--) {
                string segment = CMD_FILTER.Replace(splitPath[x].Trim(), "");
                if(segment.IsNullOrEmpty()) {
                    splitPath.RemoveAt(x);
                    removedTokens++;
                }
            }

            // Warn if the provided path has bad symbols
            if(removedTokens > 0) {
                Debug.LogWarning($@"Console command at '{path}' contained {removedTokens} invalid path " 
                    + (removedTokens > 1 ? "symbols, which were" : "symbol, which was") + " removed."); // grammar bay bee
            }

            return new ConsoleCommandPathAndArgumentBuilder(this, type, string.Join(" ", splitPath));
        }


        public void PushRegistrations() {
            foreach(KeyValuePair<string, IConsoleRegistryBranchable> command in _commandsToAdd) {
                if(INFRA.Game.Data.ConsoleCommands.Add(command.Key, command.Value))
                    Debug.Log($@"Successfully registered new console command token '{command.Key}'");
            }
            _commandsToAdd.Clear();
        }


        public class ConsoleCommandPathAndArgumentBuilder : IKwArgBuilder<ConsoleCommandPathAndArgumentBuilder>  {
            internal readonly Type _type;
            internal readonly string _path;
            internal string _desc = "No decription.";
            readonly ConsoleCommandRegistrar _prev;
            readonly List<IKwArg> args = new();
            public ConsoleCommandPathAndArgumentBuilder(ConsoleCommandRegistrar prev, Type type, string path) {
                _prev = prev;
                _type = type;
                _path = path;
            }

            public ConsoleCommandPathAndArgumentBuilder WithDescription(string desc) {
                _desc = desc;
                if(_desc.IsNullOrEmpty()) _desc = "No description.";
                return this;
            }

            public ArgumentBuilder<ConsoleCommandPathAndArgumentBuilder> Accepts(string name) { 
                return new ArgumentBuilder<ConsoleCommandPathAndArgumentBuilder>(this, name);
            }

            public ConsoleCommandRegistrar Build() {
                if(!IConsoleRegistryBranchable.IsCommandType(_type)) {
                    Debug.LogError($@"Error registering Console Command at {_path} - the registry type provided isn't a valid SimpleSyntaxCommand instance!");
                    return _prev;
                }

                SimpleSyntaxCommand newcmd = (SimpleSyntaxCommand)INFRA.SmartInstantiate(_type, _path, _desc, args.ToArray());
                _prev._commandsToAdd.TryAdd(_path, newcmd);
                return _prev;
            }

            public ConsoleCommandPathAndArgumentBuilder MakeKwArg(IKwArg arg, SaveableDataSet target) {
                args.Add(arg);
                return this;
            }
        }

        public class ArgumentBuilder<T> where T: IKwArgBuilder<T> {
            
            readonly T _root;
            readonly string _name;

            internal ArgumentBuilder(T root, string name) {
                _root = root;
                _name = name;
            }

            public KeywordArgumentBuilder<T> WithDescription(string desc) {
                return new KeywordArgumentBuilder<T>(_root, _name, desc);
            }
        }
    }


    #nullable enable
    /// <summary>
    /// The IConsoleRegistryBranchable interface and its subclasses
    /// create a branching tree structure used for tokenization.
    /// </summary>
    public interface IConsoleRegistryBranchable {

        public static bool IsCommandType(Type type) {
            return typeof(SimpleSyntaxCommand).IsAssignableFrom(type);
        }

        /// <summary>
        /// Executes this IConsoleRegistryBranchable. This method returns a 
        /// CommandExecuteResult representing its execution status.
        /// </summary>
        /// <param name="kwargs">WWOAH!</param>
        /// <returns></returns>
        public abstract ConsoleParseResult Execute(string raw, string[] kwargs);

        /// <summary>
        /// Returns the IConsoleRegistryBranchable sub-command instance mapped to the given
        /// token. If this IConsoleRegistryBranchable instance doesn't have any sub-commands
        /// (or none matching the provided token couldn't be found), this method
        /// just returns null.
        /// </summary>
        /// <param name="token"></param>
        public abstract IConsoleRegistryBranchable? GetNext(string token);

        /// <summary>
        /// Returns a list of all IConsoleRegistryBranchable sub-command instances that belong
        /// to this IConsoleRegistryBranchable. If this IConsoleRegistryBranchable instance doesn't have any
        /// sub-commands, this method just returns null.
        /// </summary>
        public abstract IEnumerable<IConsoleRegistryBranchable>? PoolExecutionBranch();

        /// <summary>
        /// Gets the ID of this IConsoleRegistryBranchable. The ID 
        /// contains the last word in the execution path.
        /// </summary>
        /// <returns></returns>
        public abstract string GetID();

        /// <summary>
        /// Adds a reference to the provided IConsoleRegistryBranchable instance to
        /// this IConsoleRegistryBranchable, sort of like a LinkedList. This structure
        /// creates a TreeMap of traversable, tokenized paths. 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract IConsoleRegistryBranchable FormOrExpandBranch(string id, IConsoleRegistryBranchable other);

        /// <summary>
        /// Gets a formatted string for printing branch information and syntax hints in the console.
        /// </summary>
        /// <returns></returns>
        public abstract string GetHelpString(int indent = 0);

        internal static string GetIndent(int count) {

            if(count <= 0) return "";
            string output = "";
            for(int x = 0; x < count; x++) 
                output += " ";
            return output;
        }
    }

    /// <summary>
    /// A container for signifying success/failure states
    /// with a message
    /// </summary>
    public class ConsoleParseResult {

        public readonly bool Succeeded;
        readonly string? _message;

        public static bool ContainsHelpString(string[] kwargs) {
            if(kwargs.IsNullOrEmpty()) return false;
            string check = kwargs[0].ToLower();
            return check.Equals("help") || check.Equals("-h");
        }

        public static ConsoleParseResult Pass(string message = "") {
            return new ConsoleParseResult(true, message);
        }

        public static ConsoleParseResult Help(IConsoleRegistryBranchable cmd) {
            return new ConsoleParseResult(true, "$::?" + cmd.GetHelpString(0));
        }

        public static ConsoleParseResult Fail(string message) {
            if(message.IsNullOrEmpty()) return FailQuiet();
            return new ConsoleParseResult(false, message);
        }

        public static ConsoleParseResult FailQuiet() {
            return new ConsoleParseResult(false, null);
        }

        public static ConsoleParseResult FailNoSuchCommand(string[] kwargs) {
            return FailNoSuchCommand(kwargs[0]);
        }

        public static ConsoleParseResult FailNoSuchCommand(string token) {
            return new ConsoleParseResult(false, $@"Invalid command '{token}.'");
        }

        private ConsoleParseResult(bool success, string? message) {
            Succeeded = success;
            _message = message;
        }

        public override string ToString() {
            return Succeeded ? "Success" : "Failure" + (_message ?? "No Message");
        }

        public void WriteToLog() {
            if(_message.IsNullOrEmpty()) return;
            if(Succeeded) Debug.Log(_message);
            else Debug.LogError("Command Error - " + _message);
        }

        public static ConsoleParseResult ProcessKeyword(IKwArg arg, string value) {
            ValueSetResult result = arg.SetValue(value);
            switch(result) {
                case ValueSetResult.SUCCESS:
                case ValueSetResult.PASS:
                    return ConsoleParseResult.Pass();
                case ValueSetResult.BAD_RANGE:
                    return ConsoleParseResult.Fail($"Argument value '{value}' is out of range for keyword '{arg.GetID()}'. Expected range {arg.GetRange()}");
                case ValueSetResult.BAD_TYPE:
                    return ConsoleParseResult.Fail($"No implicit conversion exists for argument '{arg.GetID()}' (Found '{value}', expected type {arg.GetTypeName()})");
            }
            return ConsoleParseResult.Pass();
        }
    }






    /// <summary>
    /// A ConsoleCommand whose members contain redundant branch keywords. 
    /// Ex. "teleport entity" and "teleport player." These two
    /// commands both have the "teleport" primary keyword, but "entity"
    /// and "player" are functionally classified as sub-commands, as they 
    /// fundamentally alter the manner in which the command executes.
    /// </summary>
    public class BranchingCommandSet : IConsoleRegistryBranchable {

        private readonly string _branchID;
        internal readonly Dictionary<string, IConsoleRegistryBranchable> _execBranch;
        public IReadOnlyDictionary<string, IConsoleRegistryBranchable> Branch { get => _execBranch; }

        public BranchingCommandSet(string branchID) {
            _branchID = branchID;
            _execBranch = new();
        }

        public IConsoleRegistryBranchable? GetNext(string token) {
            return _execBranch.GetValueOrDefault(token);
        }

        public IEnumerable<IConsoleRegistryBranchable> PoolExecutionBranch() {
            List<IConsoleRegistryBranchable> commands = new();
            foreach(IConsoleRegistryBranchable command in _execBranch.Values) {
                IEnumerable<IConsoleRegistryBranchable>? nextBranch = command.PoolExecutionBranch();
                if(nextBranch != null) commands.AddRange(nextBranch);
            }
            return commands;
        }

        internal bool Add(string rawpath, IConsoleRegistryBranchable command) {

            string[] splitPath = rawpath.Split(" ");

            // if the command's branch is only 1 member long, add it straight away
            if(splitPath.Length == 1) {
                if(_execBranch.ContainsKey(command.GetID())) {
                    Debug.LogWarning($@"Error adding command '{command.GetID()}' 
                        - A command with this path already exists!");
                    return false;
                }
                _execBranch[command.GetID()] = command;
                return true;
            }

            IConsoleRegistryBranchable current;
            IConsoleRegistryBranchable? next = null;

            // If the current branch doesn't exist, prime it in this set.
            if (!_execBranch.TryGetValue(splitPath[0], out current)) {
                current = new BranchingCommandSurrogate(splitPath[0]);
                _execBranch[current.GetID()] = current;
            }

            // If the command's path is only 2 members long, add it to the primed branch directly.
            if(splitPath.Length < 3) {
                current = current.FormOrExpandBranch(splitPath[^1], command);
                _execBranch[splitPath[0]] = current;
                return true;
            }

            // Iterate over each member of the path and append branches
            for(int x = 1; x < splitPath.Length - 1; x++) {
                next = current.GetNext(splitPath[x]);
                if(next == null) {
                    next = new BranchingCommandSurrogate(splitPath[x]);
                    current = current.FormOrExpandBranch(next.GetID(), next);
                }
                current = next;
            }

            // idk if this nullcheck can ever even be hit at this point
            if(next == null) {
                Debug.LogError($@"Error adding command {rawpath} to set '{GetID()}' 
                    - nullified path indicates a previous uncaught registry error.");
                return false;
            }

            next.FormOrExpandBranch(splitPath[^1], command);
            return true;
        }


        public IConsoleRegistryBranchable FormOrExpandBranch(string id, IConsoleRegistryBranchable other) {
            _execBranch[id] = other;
            return this;
        }

        public ConsoleParseResult Execute(string raw, string[] kwargs) {
            if(kwargs.IsNullOrEmpty()) return ConsoleParseResult.FailQuiet();
            IConsoleRegistryBranchable? found = GetNext(kwargs[0]);
            if(found == null) return ConsoleParseResult.FailNoSuchCommand(kwargs);
            return found.Execute(raw, kwargs.Skip(1).ToArray());
        }

        public string GetID() {
            return _branchID;
        }

        public override string ToString() {
            return GetHelpString();
        }

        public string GetHelpString(int indent = 0) {
            if(_execBranch.IsNullOrEmpty())
                return $"Dead End <color=#f52055><b>(Empty branch)</b></color>]";
            string outputAll = "";
            if(indent == 0) {
                foreach(IConsoleRegistryBranchable branch in _execBranch.Values) {
                    outputAll += $"{branch.GetHelpString(indent)}<br></color>";
                }
            } else {
                string prefix = IConsoleRegistryBranchable.GetIndent(indent + 1);
                foreach(IConsoleRegistryBranchable branch in _execBranch.Values) {
                    outputAll += $"{prefix}{branch.GetHelpString(indent)}<br></color>";
                }
            }
            return outputAll;
        }
    }

    /// <summary>
    /// A commmand branch that only has 1 member, which defers its functionality
    /// to that member.
    /// </summary>
    public class BranchingCommandSurrogate : IConsoleRegistryBranchable {

        private readonly string _branchID;
        private IConsoleRegistryBranchable? _next;

        public BranchingCommandSurrogate(IConsoleRegistryBranchable next, string branchID) {
            _branchID = branchID;
            _next = next;
        }

        public BranchingCommandSurrogate(string branchID) {
            _branchID = branchID;
            _next = null;
        }

        public ConsoleParseResult Execute(string raw, string[] kwargs) {

            if(_next == null) {
                return ConsoleParseResult.Fail($@"Command '{_branchID}' for this branch is a stub. 
                    (This indicates a registry issue, as this command has no implementation.)");
            }

            if(kwargs.IsNullOrEmpty()) {
                return ConsoleParseResult.Fail($@"Command '{raw}' not found.");
            }

            if(_next.GetID().Equals(kwargs[0]))
                return _next.Execute(raw, kwargs.Skip(1).ToArray());
            return ConsoleParseResult.FailNoSuchCommand(raw);
        }

        public string GetID() {
            return _branchID;
        }

        public IConsoleRegistryBranchable? GetNext(string token) {
            return _next;
        }

        public IEnumerable<IConsoleRegistryBranchable>? PoolExecutionBranch() {
            return _next == null ? null : new List<IConsoleRegistryBranchable>() {
                _next
            };
        }

        public IConsoleRegistryBranchable FormOrExpandBranch(string id, IConsoleRegistryBranchable other) {

            if(_next == null) {
                _next = other;
                return this;
            }

            IEnumerable<IConsoleRegistryBranchable>? branch = other.PoolExecutionBranch();
            if(branch.IsNullOrEmpty()) {
                BranchingCommandSet mutated = new BranchingCommandSet(_branchID);
                mutated._execBranch[_next.GetID()] = _next;
                mutated._execBranch[id] = other;
                return mutated;
            }

            return other.FormOrExpandBranch(_next.GetID(), _next);
        }

        public override string ToString() {
            if(_next == null)
                return "surrogate " + _branchID + " -> " + "[Dead End]";
            return "surrogate " + _branchID + " -> " + _next.ToString();
        }

        public string GetHelpString(int indent = 0) {
            string prefix = IConsoleRegistryBranchable.GetIndent(indent);
            if(_next == null)
                return $"{prefix}{GetID()}<br>{prefix}[Dead End]";
            string prefix2 = IConsoleRegistryBranchable.GetIndent(indent);
            return $"{prefix2}{_next.GetHelpString(indent)}";
        }
    }

    /// <summary>
    /// Base implementation for keyword checking. Override RunCommand() for
    /// logical implementaiton of command functionality.
    /// </summary>
    public abstract class SimpleSyntaxCommand : IConsoleRegistryBranchable {

        readonly IKwArg[] _possibleArgs;
        readonly string _cid;
        readonly string _desc;

        internal SimpleSyntaxCommand(string cid, string desc, IKwArg[] possibleArgs) {

            if(!INFRA.Game.IsLoading) {
                Debug.LogError($@"Attempted to instantiate console command {cid} in invalid loading context!
                    This console command will not be usable!");
                _possibleArgs = new IKwArg[0];
                _cid = "--";
                _desc = "INVALID";
                return;
            }

            if(cid.IsNullOrEmpty()) {
                Debug.LogError($@"Attempted to instantiate console command {cid} with an empty ID!
                    This console command will not be usable!");
                _possibleArgs = new IKwArg[0];
                _cid = "--";
                _desc = "INVALID";
                return;
            }

            _possibleArgs = possibleArgs;
            _cid = cid;
            _desc = desc;
        }

        public IConsoleRegistryBranchable FormOrExpandBranch(string id, IConsoleRegistryBranchable other) {
            IEnumerable<IConsoleRegistryBranchable>? branch = other.PoolExecutionBranch();

            if(branch == null) {
                BranchingCommandSet mutated = new BranchingCommandSet(id);
                mutated._execBranch.Add(this.GetID(), this);
                mutated._execBranch.Add(other.GetID(), other);
                return mutated;
            }

            return other.FormOrExpandBranch("", this);
        }

        protected ConsoleParseResult TrySetKwArgs(string raw, string[] kwargs) {
            if(_possibleArgs.IsNullOrEmpty() && (!kwargs.IsNullOrEmpty())) {
                string minusKwargs = raw;
                foreach(string kwarg in kwargs)
                    minusKwargs = minusKwargs.Replace(kwarg, "");
                return ConsoleParseResult.Fail($@"Command '{minusKwargs}' does not accept any keyword arguments.");
            }
            if(kwargs == null) kwargs = new string[0];
            if(kwargs.Count() != _possibleArgs.Count())
                return ConsoleParseResult.Fail($@"Incorrect number of keyword arguments - Expected {_possibleArgs.Length}, got {kwargs.Length}");
            for(int x = 0; x < kwargs.Count(); x++) {
                ConsoleParseResult attempt = ConsoleParseResult.ProcessKeyword(_possibleArgs[x], kwargs[x]);
                if(!attempt.Succeeded) return attempt;
            }
            return ConsoleParseResult.Pass();
        }

        public ConsoleParseResult Execute(string raw, string[] kwargs) {

            if(ConsoleParseResult.ContainsHelpString(kwargs))
                return ConsoleParseResult.Help(this);

            ConsoleParseResult attempt = TrySetKwArgs(raw, kwargs);
            if(attempt.Succeeded) {
                try {
                    return RunCommand(_possibleArgs);
                } catch (Exception ex) {
                    return ConsoleParseResult.Fail($@"Runtime execution failed for '{GetID()}', exception is as follows: {ex}");
                }
            }
            return attempt;
        }

        public abstract ConsoleParseResult RunCommand(IKwArg[] args);

        public string GetID() {
            string[] split = _cid.Split(" ");
            return split[split.Length - 1];
        }

        public string GetFullCommand() {
            return _cid;
        }
        
        public IConsoleRegistryBranchable? GetNext(string token) { return null; }
        public IEnumerable<IConsoleRegistryBranchable>? PoolExecutionBranch() { return null; }

        public override string ToString() {
            if(_possibleArgs.IsNullorEmpty())
                return $@"ConsoleCommand '{GetID()}' -> [No Arguments]";
            return $@"ConsoleCommand '{GetID()}' -> {_possibleArgs.ContentsToString()}";
        }

        public override bool Equals(object obj) {
            if(!(obj.GetType() is IConsoleRegistryBranchable cmd)) return false;
            return GetID().Equals(cmd.GetID());
        }

        public override int GetHashCode() {
            return GetID().GetHashCode();
        }

        public string GetHelpString(int indent = 0) {
            string prefix = IConsoleRegistryBranchable.GetIndent(indent + 1);
            string prefix2 = IConsoleRegistryBranchable.GetIndent(indent + 3);
            string prefix3 = IConsoleRegistryBranchable.GetIndent(indent + 5);
            string head = indent < 0 ? "$::cmd?:" : "";
            if(_possibleArgs.Length <= 0) 
                return $@"{prefix}{head}<color=#A2F1DB><size=18><b>{_cid}</color><color=#E3C97B> ▸</color></b></size><br>{prefix}<color=#a4b6b0><i>{_desc}</i></color><br>{prefix2}Accepts no arguments.</color><br>";
            string argstring = "";
            for(int x = 0; x < _possibleArgs.Length; x++) {
                IKwArg arg = _possibleArgs[x];
                if(arg == null) argstring += "(NULL ?? wtf)";
                else argstring += $"<br>{prefix3}<color=#a4b6b0>◇</color>{arg}";
            }
            return $"{prefix}{head}<color=#A2F1DB><size=18><b>{_cid}</color><color=#E3C97B> ▸</color></b></size><br>{prefix}<color=#a4b6b0><i>{_desc}</i></color><br>{prefix2}Accepts {_possibleArgs.Length} arguments:{argstring}</color><br>";
        }
    }
}