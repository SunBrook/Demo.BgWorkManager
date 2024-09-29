using Common.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;

namespace DisposeHub.Con
{
    public class LoadTasksLib
    {
        public List<TaskJob> _taskList = new List<TaskJob>();

        public AssemblyLoadContext _AssemblyLoadContext { get; set; }
        public Assembly _Assembly { get; set; }
        AssemblyDependencyResolver resolver { get; set; }

        public string filePath = string.Empty;

        public bool LoadFile(string filePath)
        {
            this.filePath = filePath;
            try
            {
                resolver = new AssemblyDependencyResolver(filePath);
                _AssemblyLoadContext = new AssemblyLoadContext(Guid.NewGuid().ToString("N"), true);
                _AssemblyLoadContext.Resolving += _AssemblyLoadContext_Resolving;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var _Assembly = _AssemblyLoadContext.LoadFromStream(fs);
                    var Modules = _Assembly.Modules;
                    foreach (var item in _Assembly.GetTypes())
                    {
                        if (item.GetMethod("Work") != null)
                        {
                            var task = (TaskJob)Activator.CreateInstance(item);
                            if (task != null)
                            {
                                _taskList.Add(task);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadFile:{ex}");
            }
            return false;
        }

        private Assembly? _AssemblyLoadContext_Resolving(AssemblyLoadContext context, AssemblyName name)
        {
            Console.WriteLine($"加载：{name.Name}");
            var path = resolver.ResolveAssemblyToPath(name);
            if (!string.IsNullOrEmpty(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return _AssemblyLoadContext.LoadFromStream(fs);
                }
            }
            return null;
        }
    }
}
