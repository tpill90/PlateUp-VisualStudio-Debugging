using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Mdb;

namespace PortablePdbToMdb;

public class PortablePdbToMdb : Task
{
    [Required]
    public string? AssemblyPath { get; set; }

    public override bool Execute()
    {
        try
        {
            var module = ModuleDefinition.ReadModule(AssemblyPath, new()
            {
                ReadSymbols =  true,
            });

            var allMethod = module.GetTypes().SelectMany(x => x.Methods);
            var writerProvider = new MdbWriterProvider();
            using (var writer = writerProvider.GetSymbolWriter(module, AssemblyPath))
            {
                foreach (MethodDefinition methodDefinition in allMethod)
                    writer.Write(methodDefinition.DebugInformation);
            }
        }
        catch (Exception e)
        {
            Log.LogError(e.Message);
            return false;
        }

        string mdbPath = AssemblyPath + ".mdb";
        if (File.Exists(mdbPath))
            Log.LogMessage(MessageImportance.High, $"Mdb file created successfully at {mdbPath}");
        return true;
    }
}


// var assemblyFile = args[0];

// 			var resolver = new DefaultAssemblyResolver();
// 			resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFile));

// 			var readerParams = new ReaderParameters {
// 				AssemblyResolver = resolver,
// 				ReadSymbols = true,
// 				ReadWrite = true,
// 				SymbolReaderProvider = new PdbReaderProvider(),
// 			};

// 			using (var assembly = AssemblyDefinition.ReadAssembly(assemblyFile, readerParams))
// 			{
// 				var writerParams = new WriterParameters {
// 					SymbolWriterProvider = new MdbWriterProvider(),
// 					WriteSymbols = true,
// 				};

// 				assembly.Write(writerParams);
// 			}

// 			Console.WriteLine("Done");
// 			return 0;