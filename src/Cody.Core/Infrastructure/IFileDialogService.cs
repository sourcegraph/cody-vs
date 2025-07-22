using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IFileDialogService
    {
        string ShowSaveFileDialog(string initialPath, string title, IReadOnlyDictionary<string, string[]> filters);
    }
}
