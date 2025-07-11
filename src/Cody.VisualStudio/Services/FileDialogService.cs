using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class FileDialogService : IFileDialogService
    {
        private readonly ISolutionService solutionService;
        private readonly ILog log;

        public FileDialogService(ISolutionService solutionService, ILog log)
        {
            this.solutionService = solutionService;
            this.log = log;
        }

        public string ShowSaveFileDialog(string initialPath, string title, IReadOnlyDictionary<string, string[]> filters)
        {
            var filter = BuildFilterString(filters);

            var initialFileName = Path.GetFileName(initialPath);
            if (initialFileName == null || !initialFileName.Contains(".")) initialFileName = "Untitled";
            if (string.IsNullOrEmpty(initialPath)) initialPath = solutionService.GetSolutionDirectory();

            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = Path.GetDirectoryName(initialPath);
                dlg.Filter = filter;
                dlg.FileName = initialFileName;
                dlg.Title = title ?? "Cody: Save as New File";

                var dialogResult = dlg.ShowDialog();
                if (dialogResult == true)
                {
                    return dlg.FileName;
                }

                return null;
            });

            return result;

        }

        private string BuildFilterString(IReadOnlyDictionary<string, string[]> filters)
        {
            if (filters == null) return string.Empty;

            var filterList = new List<string>(1);
            foreach (var filter in filters)
            {
                var singleFilter = $"{filter.Key}|{string.Join(";", filter.Value.Select(x => $"*.{x}"))}";
                filterList.Add(singleFilter);
            }

            return string.Join("|", filterList);
        }
    }
}
