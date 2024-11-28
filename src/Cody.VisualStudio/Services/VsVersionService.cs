using Cody.Core.Ide;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cody.Core.Logging;

namespace Cody.VisualStudio.Services
{
    public class VsVersionService : IVsVersionService
    {
        private readonly ILog _logger;

        public VsVersionService(ILog logger)
        {
            _logger = logger;

            SemanticVersion = GetAppIdStringProperty(VSAPropID.ProductSemanticVersion);
            DisplayVersion = GetAppIdStringProperty(VSAPropID.ProductDisplayVersion);
            EditionName = GetAppIdStringProperty(VSAPropID.EditionName);
        }


        public string SemanticVersion { get; }

        public string DisplayVersion { get; }

        public string EditionName { get; }


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
