
// SIGNATURE :)

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.quatworks.INFRASEC.Data.Console {

    [RegistrySubscriber]
    public class CVarCommands {

        [ConsoleCommandRegistry]
        public static void RegisterCommands(ConsoleCommandRegistrar commands) {
            commands
            .New(typeof(CMD_CvarSet), "cvar set")
                .WithDescription("Allows users to change the values of globally-stored console variables")
                .Accepts("name")
                    .WithDescription("The name of the cvar to modify")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Accepts("arg")
                    .WithDescription("The new value of the cvar")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Build()
            .New(typeof(CMD_CvarGet), "cvar get")
                .WithDescription("Displays the value of the specified console variable")
                .Accepts("name")
                    .WithDescription("The name of the cvar to access")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Build()
            .New(typeof(CMD_CvarList), "cvar list")
                .WithDescription("Lists all console variables in the current session")
                .Build()
            .New(typeof(CMD_CvarReset), "cvar reset")
                .WithDescription("Resets the given ConsoleVariable to its default")
                .Accepts("name")
                    .WithDescription("The name of the cvar to modify")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Build()
            .New(typeof(CMD_CvarPush), "cvar push")
                .WithDescription("Pushes cvar changes to disk")
                .Build()
            .New(typeof(CMD_Help), "help")
                .WithDescription("Lists all commands available in the console")
                .Build()
            .New(typeof(CMD_LoadScene), "loadscene")
                .WithDescription("Loads a scene/level based on the provided name")
                .Accepts("name")
                    .WithDescription("The name of the cvar to modify")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Build()
            .New(typeof(CMD_Exit), "exit")
                .WithDescription("Literally just closes the game")
                .Build();
        }
    }

    public class CMD_CvarSet : SimpleSyntaxCommand {
        public CMD_CvarSet(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            IKwArg arg = INFRA.Game.Data.SearchForArgument(args[0].GetString());
            if(arg == null) return ConsoleParseResult.Fail($"KeywordArgument '{args[0].GetString()}' couldn't be found.");
            string oldv = $"<color=#afff77>{arg.GetString()}</color>";
            arg.SetValue(args[1].GetString());
            return ConsoleParseResult.Pass($"$::{arg} <br> Set: {oldv} -> <color=#77ff95>{arg.GetString()}</color>");
        }
    }

    public class CMD_CvarGet : SimpleSyntaxCommand {
        public CMD_CvarGet(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            IKwArg arg = INFRA.Game.Data.SearchForArgument(args[0].GetString());
            if(arg == null) return ConsoleParseResult.Fail($"KeywordArgument '{args[0].GetString()}' couldn't be found.");
            return ConsoleParseResult.Pass($"$::{arg} <br> Value: <color=#77ff95>{arg.GetString()}</color>");
        }
    }

    public class CMD_CvarList : SimpleSyntaxCommand {
        public CMD_CvarList(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            return ConsoleParseResult.Pass(INFRA.Game.Data.GetCVarsAsString());
        }
    }

    public class CMD_CvarReset : SimpleSyntaxCommand {
        public CMD_CvarReset(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {

            if(args[0].GetString().Equals("*")) {
                int changed = INFRA.Game.Data.ResetAllArguments();
                return ConsoleParseResult.Pass($"Reset {changed} arguments to their defaults.");
            }

            IKwArg arg = INFRA.Game.Data.SearchForArgument(args[0].GetString());
            if(arg == null) return ConsoleParseResult.Fail($"KeywordArgument '{args[0].GetString()}' couldn't be found.");
            string oldv = $"<color=#afff77>{arg.GetString()}</color>";
            arg.Reset();
            return ConsoleParseResult.Pass($"$::{arg} <br> Set: {oldv} -> <color=#77ff95>{arg.GetString()}</color> (Default)");
        }
    }

    public class CMD_CvarPush : SimpleSyntaxCommand {
        public CMD_CvarPush(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            INFRA.Game.Data.ForceWriteAll();
            return ConsoleParseResult.Pass("Updated cvars.");
        }
    }

    public class CMD_Help : SimpleSyntaxCommand {
        public CMD_Help(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            return ConsoleParseResult.Pass($"<br>{INFRA.Game.Console.GetFormattedVersion()}<br>{INFRA.Game.Data.GetCommandsAsString()}");
        }
    }

    public class CMD_LoadScene : SimpleSyntaxCommand {
        public CMD_LoadScene(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            SceneManager.LoadSceneAsync(args[0].GetString(), LoadSceneMode.Single);
            INFRA.Game.LockCursor();
            INFRA.Game.Console.OverrideCursorLock();
            return ConsoleParseResult.Pass($"Attempting to load scene '{args[0].GetString()}'");
        }
    }


    public class CMD_Exit : SimpleSyntaxCommand {
        public CMD_Exit(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}
        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            Application.Quit();
            return ConsoleParseResult.Pass($"Attempting to load scene '{args[0].GetString()}'");
        }
    }
}