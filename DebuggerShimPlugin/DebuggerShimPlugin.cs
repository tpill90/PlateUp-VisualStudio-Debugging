using System.Diagnostics;

namespace DebuggerShimPlugin
{
    [UsedImplicitly]
    [BepInPlugin($"tpill90.{ModInfo.ModName}", ModInfo.ModName, ModInfo.ModVersion)]
    public class DebuggerShimPlugin : BaseUnityPlugin
    {
        public static ManualLogSource log;

        [UsedImplicitly]
        public void Awake()
        {
            // TODO I should consider scanning the workshop dir and loading those dlls first
            // TODO I should also consider scanning the mods dir instead of requiring mods to be copied here
            // TODO it is currently going to be a requirement that .pdb's are copied over to the appropriate directory
            log = Logger;
            log.LogInfo("Loading BepinEx Debugger Shim");

            var plateUpAppDir = Directory.GetCurrentDirectory();
            var bepinExDir = Path.Combine(plateUpAppDir, "BepInEx/Plugins");
            var pdb2mdbPath = Path.Combine(plateUpAppDir, "Libraries/pdb2mdb.exe");

            // Getting dlls in plugin dir
            var directoryInfo = new DirectoryInfo(bepinExDir);
            var filesInDir = directoryInfo.GetFiles("*.dll")
                                          .Where(e => !e.Name.Contains(nameof(DebuggerShimPlugin)))
                                          .ToList();

            // Dynamically loading each mod here prior to the game attempting to load the mod itself.
            // This allows us to debug mods because Bepinex will pull the mods into its AppDomain, and will correctly read the .mdb debug symbols
            log.LogInfo($"Found {filesInDir.Count} mod .dlls");
            foreach (var file in filesInDir)
            {
                GenerateMdb(file, pdb2mdbPath);

                Assembly assembly = Assembly.LoadFrom(file.FullName);
                log.LogInfo($"      Loaded {assembly.FullName}");


            }
        }

        private void GenerateMdb(FileInfo file, string pdbToMdbPath)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = pdbToMdbPath,
                    Arguments = $"\"{file.FullName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}