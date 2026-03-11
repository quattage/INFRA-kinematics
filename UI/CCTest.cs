
// SIGNATURE :)


using Assets.quatworks.INFRASEC.Data;
using Assets.quatworks.INFRASEC.Data.Console;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.UI {

    [RegistrySubscriber]
    public class CCTest {

        [ConsoleCommandRegistry]
        public static void RegisterCommands(ConsoleCommandRegistrar commands) {
            commands
            .New(typeof(ConsoleCommandTest), "console penis test")
                .WithDescription("Prints 'penis' a bunch of times in the console. wahoo.")
                .Accepts("count")
                    .WithDescription("the amount of penis to type")
                    .AsInt()
                    .DefaultsTo(1)
                    .WithMinimum(1)
                    .WithMaximum(16)
                    .Make()
                .Accepts("use_commas")
                    .WithDescription("whether or not to seperate penis with commas")
                    .AsBool()
                    .DefaultsTo(true)
                    .Make()
                .Build();
        }
    }

    public class ConsoleCommandTest : SimpleSyntaxCommand {

        public ConsoleCommandTest(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}

        public override ConsoleParseResult RunCommand(IKwArg[] args) {
            int count = args[0].GetInt();
            bool use_commas = args[1].GetBool();
            string output = "[";
            for(int x = 0; x < count; x++) {
                output += "penis";
                if(x < count - 1)
                    output += use_commas ? ", " : " ";
            }
            Debug.Log(output + "!]");
            return ConsoleParseResult.Pass();
        }
    }
}