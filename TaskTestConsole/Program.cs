// See https://aka.ms/new-console-template for more information
using System.Reflection;
using TaskTestConsole.Builder;



var fileDict = new Dictionary<string, string>
{
    { "CircleTask.cs", "E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\CircleTask.cs"},
    { "ConsumeTask.cs", "E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\Demo\\ConsumeTask.cs" },
    { "TempTask.cs", "E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\Demo\\TempTask.cs" }
};

foreach (var fileName in fileDict.Keys)
{
    var filePath = fileDict[fileName];
    Compiler.Compile(fileName, filePath);
}

//var filepath = "E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\CircleTask.cs";
//Compiler.Compile(filepath);


return;
Assembly assembly = Assembly.LoadFrom(Path.Combine("E:\\test\\Demo.BgWorkManager\\Tasks.Lib\\ReleaseDLL", "CircleTask.dll"));
var obj = assembly.CreateInstance("Tasks.Lib.CircleTask");

var method = obj.GetType().GetMethod("Work", new Type[] { typeof(string) });
method.Invoke(obj, new object[] { "123" });

Console.ReadLine();
