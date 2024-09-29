using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Tasks.Lib;

namespace TaskTestConsole.Builder;

internal static class Compiler
{
    public static byte[] Compile(string fileName, string filepath)
    {
        Console.WriteLine($"Starting compilation of: '{filepath}'");

        var sourceCode = File.ReadAllText(filepath);

        using var peStream = new MemoryStream();
        var result = GenerateCode(sourceCode, fileName).Emit(peStream);

        if (!result.Success)
        {
            Console.WriteLine("Compilation done with error.");

            var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
            }

            return null;
        }

        Console.WriteLine("Compilation done without any error.");

        peStream.Seek(0, SeekOrigin.Begin);

        return peStream.ToArray();
    }

    private static CSharpCompilation GenerateCode(string sourceCode, string fileName)
    {
        var codeString = SourceText.From(sourceCode);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);

        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

        // ÃÌº”“˝”√
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        };

        Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

        var files = Directory.GetFiles(Path.Combine("E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\RefDLL"));

        foreach (var file in files)
        {
            references.Add(MetadataReference.CreateFromFile(Path.Combine("E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\RefDLL", file)));
        }

        var result = CSharpCompilation.Create(fileName,
            new[] { parsedSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        EmitResult emitResult;
        byte[] dllBytes;
        using (var stream = new MemoryStream())
        {
            emitResult = result.Emit(stream);
            dllBytes = stream.ToArray();
        }

        File.WriteAllBytes(Path.Combine("E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\ReleaseDLL", fileName), dllBytes);

        return result;
    }
}