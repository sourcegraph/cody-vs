using Microsoft.VisualStudio.Shell.Connected.CredentialStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.AgentTester
{
    public class FakeSecretStorageProvider: IVsCredentialStorageService
    {
        public IVsCredential Add(IVsCredentialKey key, string value)
        {
            throw new NotImplementedException();
        }
        public IVsCredential Retrieve(IVsCredentialKey key)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<IVsCredential> RetrieveAll(string key)
        {
            throw new NotImplementedException();
        }
        public bool Remove(IVsCredentialKey key)
        {
            throw new NotImplementedException();
        }
        public IVsCredentialKey CreateCredentialKey(string a, string b, string c, string d)
        {
            throw new NotImplementedException();
        }
    }
}
