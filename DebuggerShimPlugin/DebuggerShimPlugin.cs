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
            log = Logger;
            log.LogInfo("Loading BepinEx Debugger Shim");

            // TODO this needs to be determined dynamically at runtime because it can and will likely change
            var plateUpAppDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\PlateUp\\PlateUp";
            var modsDir = Path.Combine(plateUpAppDir, "Mods");
            var pdb2mdbPath = Path.Combine(plateUpAppDir, "Libraries/pdb2mdb.exe");

            if (!Directory.Exists(modsDir))
            {
                log.LogWarning("No mods dir found!  Nothing to load!");
                return;
            }

            // Finding all .dlls
            var directoryInfo = new DirectoryInfo(modsDir);
            var filesInDir = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories)
                                          .ToList();

            // Dynamically loading each mod here prior to the game attempting to load the mod itself.
            // This allows us to debug mods because Bepinex will pull the mods into its AppDomain, and will correctly read the .mdb debug symbols
            log.LogInfo($"Found {filesInDir.Count} mod .dlls");
            foreach (var file in filesInDir)
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(file.FullName);
                var pdbPath = $"{file.Directory}\\{fileNameNoExtension}.pdb";

                // Can't generate an .mdb if there isn't a .pdb
                if (!File.Exists(pdbPath))
                {
                    log.LogError($"pdb not found for {file.Name}.  Will not be able to debug in Visual Studio.  " +
                                  $"The .pdb must be generated as a 'Full' .pdb and copied into the mod directory.");
                }

                if (File.Exists(pdbPath))
                {
                    GenerateMdbManaged(file);
                }

                Assembly assembly = Assembly.LoadFrom(file.FullName);
                log.LogInfo($"Loaded {assembly.FullName}");
            }
        }

        //TODO refactor this signature + general refactor + comment
        private static void GenerateMdbManaged(FileInfo file)
        {
            var fileNameNoExtension = Path.GetFileNameWithoutExtension(file.FullName);
            FileInfo pdbFileInfo = new FileInfo($"{file.Directory}\\{fileNameNoExtension}.pdb");
            FileInfo mdbFileInfo = new FileInfo($"{file.Directory}\\{fileNameNoExtension}.dll.mdb");

            if (mdbFileInfo.Exists && pdbFileInfo.LastWriteTime < mdbFileInfo.LastWriteTime)
            {
                return;
            }

            var module = ModuleDefinition.ReadModule(file.FullName, new()
            {
                ReadSymbols = true
            });

            var allMethod = module.GetTypes().SelectMany(x => x.Methods);
            var writerProvider = new MdbWriterProvider();
            using (var writer = writerProvider.GetSymbolWriter(module, file.FullName))
            {
                foreach (MethodDefinition methodDefinition in allMethod)
                {
                    writer.Write(methodDefinition.DebugInformation);
                }
            }

            log.LogWarning($"Generated new .mdb for {file.Name}");
        }
    }
}