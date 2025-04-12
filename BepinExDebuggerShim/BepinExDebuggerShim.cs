using System;
using System.IO;
using System.Linq;

namespace BepinExDebuggerShim
{
    [UsedImplicitly]
    [BepInPlugin($"tpill90.{ModInfo.ModName}", ModInfo.ModName, ModInfo.ModVersion)]
    public class BepinExDebuggerShim : BaseUnityPlugin
    {
        public static ManualLogSource log;

        [UsedImplicitly]
        public void Awake()
        {
            log = Logger;
            log.LogInfo("Loading BepinEx Debugger Shim");

            var plateUpAppDir = Directory.GetCurrentDirectory();
            var bepinExDir = Path.Combine(plateUpAppDir, "BepInEx/Plugins");

            // Getting dlls in plugin dir
            var directoryInfo = new DirectoryInfo(bepinExDir);
            var filesInDir = directoryInfo.GetFiles("*.dll")
                                          .Where(e => !e.Name.Contains(nameof(BepinExDebuggerShim)))
                                          .ToList();

            // Dynamically loading each mod here prior to the game attempting to load the mod itself.
            // This allows us to debug mods because Bepinex will pull the mods into its AppDomain, and will correctly read the .mdb debug symbols
            log.LogInfo($"Found {filesInDir.Count} mod .dlls");
            foreach (var file in filesInDir)
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                log.LogInfo($"      Loaded {assembly.FullName}");
            }

        }
    }
}