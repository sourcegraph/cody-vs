using Cody.Core.Infrastructure;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.VisualStudio.Services
{
    public class VsFolderStoreService : IVsFolderStoreService
    {
        private readonly IVsSolution vsSolution;
        private readonly ILog logger;

        public VsFolderStoreService(IVsSolution vsSolution, ILog logger)
        {
            this.vsSolution = vsSolution;
            this.logger = logger;
        }

        private string GetVsFolder()
        {
            vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionDirectory, out object value);

            if (value == null) return null;

            return Path.Combine((string)value, ".vs");
        }

        public bool SaveData(string fileName, object data)
        {
            try
            {
                var path = GetVsFolder();
                var json = JsonConvert.SerializeObject(data);
                var filePath = Path.Combine(path, fileName);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Can't save data in .vs folder", ex);
            }

            return false;

        }

        public T LoadData<T>(string fileName) where T : class
        {
            try
            {
                var path = GetVsFolder();
                var filePath = Path.Combine(path, fileName);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var obj = JsonConvert.DeserializeObject<T>(json);

                    return obj;
                }

                logger.Info($"File '{filePath}' does not exist");
            }
            catch (Exception ex)
            {
                logger.Error("Can't load data from .vs folder", ex);
            }

            return null;
        }
    }
}
