using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Tests
{
    public static class SolutionsPaths
    {
        public static string ConsoleApp1Dir => new DirectoryInfo(@"..\..\TestProjects\ConsoleApp1\").FullName;

        public static string GetConsoleApp1File(string path) => Path.Combine(ConsoleApp1Dir, path);
    }
}
