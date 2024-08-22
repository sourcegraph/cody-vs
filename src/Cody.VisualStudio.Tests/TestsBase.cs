﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cody.VisualStudio.Tests
{
    public abstract class TestsBase
    {
        protected async Task<CodyPackage> GetPackageAsync()
        {
            var guid = CodyPackage.PackageGuidString;
            var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            var codyPackage = (CodyPackage)await shell.LoadPackageAsync(new Guid(guid)); // forces to load CodyPackage, even when the Tool Window is not selected

            return codyPackage;
        }
    }
}