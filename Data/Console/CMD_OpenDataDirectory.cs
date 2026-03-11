
// SIGNATURE :)

using System.Diagnostics;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Data.Console {

    [RegistrySubscriber]
    public class CMD_OpenDataDirectory : SimpleSyntaxCommand {

        [ConsoleCommandRegistry]
        public static void RegisterCommand(ConsoleCommandRegistrar commands) {
            commands
            .New(typeof(CMD_OpenDataDirectory), "opendir")
                .WithDescription("Opens INFRASEC's data and settings in the file system.")
                .Build();
        }

        public CMD_OpenDataDirectory(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}


        public override ConsoleParseResult RunCommand(IKwArg[] args) {

            string path = Application.persistentDataPath.Replace("/", "\\");

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
                Process.Start("explorer.exe", path);
                return ConsoleParseResult.Pass("<color=#f57220>Opening data directory...</color>");
            }

            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
                Process.Start("open", path);
                return ConsoleParseResult.Pass("<color=#f57220>Opening data directory...</color>");
            }

            if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor) {
                Process.Start("xdg-open", path);
                return ConsoleParseResult.Pass("<color=#f57220>Opening data directory...</color>");
            }

            return ConsoleParseResult.Fail("Unsupported runtime platform");
        }
    }
}