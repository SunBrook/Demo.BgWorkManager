using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;

namespace Tasks.Lib.Builder;

internal static class Compiler
{
    private const string BASIC_FILE_PATH = "C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\6.0.28\\ref\\net6.0";
    private static string[] BASIC_REFERENCE_NAMES = new string[] { "System.Linq.dll", "System.ComponentModel.TypeConverter.dll" };
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

        // 添加引用
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        };

        Assembly.GetEntryAssembly()?.GetReferencedAssemblies().ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

        var fileNamePathMappings = new Dictionary<string, string>();

        DirectoryInfo basicDirectoryInfo = new DirectoryInfo(BASIC_FILE_PATH);

        var baseFiles = basicDirectoryInfo.GetFiles().Where(x => BASIC_REFERENCE_NAMES.Any(r => r == x.Name));
        foreach (var file in baseFiles)
        {
            if (!fileNamePathMappings.ContainsKey(file.Name))
            {
                fileNamePathMappings[file.Name] = file.FullName;
            }
        }
        //  添加bin文件夹下的引用
        DirectoryInfo binDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        var binFiles = binDirectoryInfo.GetFiles().Where(x => x.Name.EndsWith(".dll") && x.Name != "Tasks.Lib.dll");
        foreach (var file in binFiles)
        {
            if (!fileNamePathMappings.ContainsKey(file.Name))
            {
                fileNamePathMappings[file.Name] = file.FullName;
            }
        }

        foreach (var keyValue in fileNamePathMappings)
        {
            references.Add(MetadataReference.CreateFromFile(keyValue.Value));
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