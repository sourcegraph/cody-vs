using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using Cody.Core.Logging;

namespace Cody.VisualStudio.Services
{
    public class SolutionService : ISolutionService
    {
        private readonly IVsSolution _vsSolution;
        private readonly ILog _logger;

        public SolutionService(IVsSolution solutionService, ILog logger)
        {
            _vsSolution = solutionService;
            _logger = logger;
        }

        public bool IsSolutionOpen()
        {
            _vsSolution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value);
            var isOpened = value is bool isSolutionOpen && isSolutionOpen;

            _logger.Debug($"Is solution opened:{isOpened}");

            return isOpened;
        }

        public string GetSolutionDirectory()
        {
            _vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out object value);
            var solutionDir = Path.GetDirectoryName(value as string);

            // Use user directory if solution directory is null.
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            _logger.Debug($"'{solutionDir}'");

            return new Uri(solutionDir ?? userDir).ToString();
        }
    }
}
