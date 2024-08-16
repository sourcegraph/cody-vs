using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;

namespace Cody.VisualStudio.Services
{
    public class SolutionService : ISolutionService
    {
        private IVsSolution solutionService;

        public SolutionService(IVsSolution solutionService)
        {
            this.solutionService = solutionService;
        }

        public bool IsSolutionOpen()
        {
            solutionService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value);
            return value is bool isSolutionOpen && isSolutionOpen;
        }

        public string GetSolutionDirectory()
        {
            solutionService.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out object value);
            var solutionDir = Path.GetDirectoryName(value as string);

            // Use user directory if solution directory is null.
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            Debug.WriteLine(solutionDir, "GetSolutionDirectory");

            return new Uri(solutionDir ?? userDir).ToString();
        }
    }
}
