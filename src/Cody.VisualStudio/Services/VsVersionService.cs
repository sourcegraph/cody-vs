using System;
using Cody.Core.Ide;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Shell.Interop;

#pragma warning disable VSTHRD010

namespace Cody.VisualStudio.Services
{
    public class VsVersionService : IVsVersionService
    {
        private readonly ILog _logger;
        private Version _version;

        public VsVersionService(ILog logger)
        {
            _logger = logger;

        }

        private Version GetVersion()
        {
            try
            {
                if (_version != null)
                    return _version;

                var shell = (IVsShell)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShell));
                shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object value);
                if (value is string raw)
                {
                    _logger.Debug($"VS Raw Version:{raw}");

                    _version = Version.Parse(raw.Split(' ')[0]);
                    return _version;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot get VS version from IVsShell.", ex);
            }

            return null;
        }

        public Version Version => GetVersion();
    }
}
