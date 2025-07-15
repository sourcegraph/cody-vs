using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class ProjectItem
    {
        private IVsProject2 vsProject;

        public ProjectItem(IVsProject2 vsProject)
        {
            this.vsProject = vsProject;
        }

        public string FilePath
        {
            get
            {
                vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out string projectPath);
                return projectPath;
            }
        }

        public string Directory => Path.GetDirectoryName(FilePath);


        public bool IsFileInProject(string filePath)
        {
            int hr = vsProject.IsDocumentInProject(filePath, out int found, new VSDOCUMENTPRIORITY[1], out uint itemId);
            return hr == VSConstants.S_OK && found != 0;
        }

        public bool IsFileUnderProjectDirectory(string filePath)
        {
            if (Directory == null) return false;

            var normalizedFilePath = Path.GetFullPath(filePath);
            var normalizedDirPath = Path.GetFullPath(Directory);

            if (!normalizedDirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalizedDirPath += Path.DirectorySeparatorChar;

            var directoryUri = new Uri(normalizedDirPath);
            var fileUri = new Uri(normalizedFilePath);

            return directoryUri.IsBaseOf(fileUri);
        }

        public bool AddFile(string filePath)
        {
            var relativePath = GetRelativePath(Directory, filePath);
            var result = new VSADDRESULT[1];

            var hr = vsProject.AddItem(
                VSConstants.VSITEMID_ROOT,
                VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE,
                relativePath,
                1,
                new string[] { filePath },
                IntPtr.Zero,
                result);

            return hr == VSConstants.S_OK && result[0] == VSADDRESULT.ADDRESULT_Success;
        }

        public bool RemoveFile(string filePath)
        {
            int hr = vsProject.IsDocumentInProject(filePath, out int found, new VSDOCUMENTPRIORITY[1], out uint itemId);
            if (hr == VSConstants.S_OK && found != 0)
            {
                hr = vsProject.RemoveItem(0, itemId, out int result);
                if (hr == VSConstants.S_OK && result != 0) return true;
            }

            return false;
        }

        private string GetRelativePath(string basePath, string targetPath)
        {
            string absoluteBasePath = Path.GetFullPath(basePath);
            string absoluteTargetPath = Path.GetFullPath(targetPath);

            if (!absoluteBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                absoluteBasePath += Path.DirectorySeparatorChar;

            var baseUri = new Uri(absoluteBasePath);
            var targetUri = new Uri(absoluteTargetPath);

            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

    }
}
