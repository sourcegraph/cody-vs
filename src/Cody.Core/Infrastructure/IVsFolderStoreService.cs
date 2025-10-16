using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IVsFolderStoreService
    {
        bool SaveData(string fileName, object data);
        T LoadData<T>(string fileName) where T : class;
    }
}
