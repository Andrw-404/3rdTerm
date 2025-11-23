using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyNUnit
{
    public class TestRunner
    {
        public void RunTest(string path)
        {
            var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            var results = new ConcurrentBag<TestResult>();

            Parallel.ForEach(dlls, (dllPath) =>
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var testClasses = assembly.GetTypes();
                }
            }

        }

    }
}
