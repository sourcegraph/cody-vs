using Cody.Core.Infrastructure;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            solutionService.GetProperty((int)__VSPROPID.VSPROPID_SolutionDirectory, out object value);
            return value as string;
        }
    }
}
