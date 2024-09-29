@echo off
:: 设置环境变量，系统配置 CSC_PATH VS安装路径到Roslyn
set CSC_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin\Roslyn
if exist "%CSC_PATH%\csc.exe" (
    :: /t:library 任务.cs /out: 输出dll路径 /r: 依赖的DLL文件
    :: 其他系统dll在 C:\Windows\Microsoft.NET\Framework64\v4.0.30319 复制出来
    "%CSC_PATH%\csc.exe" /nostdlib /t:library Demo/ConsumeTask.cs /out:./ReleaseDLL/ConsumeTask.dll ^
        /r:./RefDLL/Common.Lib.dll ^
        /r:./RefDLL/DisposeHub.Con.dll ^
        /r:./RefDLL/Microsoft.EntityFrameworkCore.dll ^
        /r:./RefDLL/System.Collections.dll ^
        /r:./RefDLL/System.ComponentModel.TypeConverter.dll ^
        /r:./RefDLL/System.Console.dll ^
        /r:./RefDLL/System.Linq.dll ^
        /r:./RefDLL/System.Linq.Expressions.dll ^
        /r:./RefDLL/System.Linq.Queryable.dll ^
        /r:./RefDLL/System.Private.CoreLib.dll ^
        /r:./RefDLL/System.Private.Uri.dll ^
        /r:./RefDLL/System.Runtime.CompilerServices.Unsafe.dll ^
        /r:./RefDLL/System.Runtime.dll ^
        /r:./RefDLL/System.Runtime.InteropServices.dll ^
        /r:./RefDLL/System.Threading.Thread.dll
        
        
    :: "%CSC_PATH%\csc.exe" /t:library ./Demo/TempTask.cs /out:./ReleaseDLL/TempTask.dll /r:./RefDLL/DisposeHub.Con.dll /r:./RefDLL/Common.Lib.dll /r:./RefDLL/System.Runtime.dll
    :: "%CSC_PATH%\csc.exe" /t:library ./Demo/ConsumeTask.cs /out:./ReleaseDLL/ConsumeTask.dll /r:./RefDLL/DisposeHub.Con.dll /r:./RefDLL/System.Collections.dll /r:./RefDLL/Common.Lib.dll /r:./RefDLL/System.Runtime.dll
) else (
    echo Cannot find csc.exe at %CSC_PATH%
)