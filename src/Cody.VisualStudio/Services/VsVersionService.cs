using Cody.Core.Ide;
using Cody.Core.Logging;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cody.VisualStudio.Services
{
    public class VsVersionService : IVsVersionService
    {
        private readonly ILog _logger;

        public VsVersionService(ILog logger)
        {
            _logger = logger;

            try
            {
                SemanticVersion = GetAppIdStringProperty(VSAPropID.ProductSemanticVersion);
                DisplayVersion = GetAppIdStringProperty(VSAPropID.ProductDisplayVersion);
                EditionName = GetAppIdStringProperty(VSAPropID.EditionName);

                Version = ParseVersion(DisplayVersion);
            }
            catch (Exception ex)
            {
                _logger.Error("Retrieving VS version failed.", ex);
            }
        }

        private Version ParseVersion(string version)
        {
            int spaceIndex = version.IndexOf(' ');
            if (spaceIndex >= 0) version = version.Substring(0, spaceIndex).Trim();

            return Version.Parse(version);
        }


        public string SemanticVersion { get; }

        public string DisplayVersion { get; }

        public string EditionName { get; }

        public Version Version { get; }

        public bool HasCompletionSupport
        {
            get
            {
                var minimalVersion = new Version(17, 6);

                if (Version >= minimalVersion)
                    return true;
                return false;
            }
        }

        [Guid("1EAA526A-0898-11d3-B868-00C04F79F802")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
#pragma warning disable IDE1006 // Naming Styles
        public interface SVsAppId
#pragma warning restore IDE1006 // Naming Styles
        {
        }

        [Guid("1EAA526A-0898-11d3-B868-00C04F79F802")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        public interface IVsAppId
        {
            [MethodImpl(MethodImplOptions.PreserveSig)]
            int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider pSP);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int GetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] out object pvar);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int SetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] object var);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int GetGuidProperty(int propid, out Guid guid);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int SetGuidProperty(int propid, ref Guid rguid);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int Initialize();
        }

        //https://github.com/dotnet/project-system/blob/main/src/Microsoft.VisualStudio.ProjectSystem.Managed.VS/ProjectSystem/VS/Interop/IVsAppId.cs
        private enum VSAPropID
        {
            ProductSemanticVersion = -8642,
            ProductDisplayVersion = -8641,
            EditionName = -8620
        }

        private string GetAppIdStringProperty(VSAPropID propertyId)
        {
            try
            {
                var vsAppId = ServiceProvider.GlobalProvider.GetService(typeof(IVsAppId)) as IVsAppId;

                if (vsAppId == null)
                {
                    _logger.Error($"Cannot get {nameof(IVsAppId)}");
                    return null;
                }

                vsAppId.GetProperty((int)propertyId, out object value);
                return value as string;
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot get VS version from IVsAppId.", ex);
            }

            return null;
        }
    }
}
