using System;
using System.Reflection;

namespace Cody.Core.Inf
{
    public class VersionService : IVersionService
    {
        private readonly Version _version;

        public VersionService()
        {
            _version = Assembly.GetExecutingAssembly().GetName().Version;
            BuildDate = new DateTime(2000, 01, 01).AddDays(_version.Build).AddSeconds(_version.Revision * 2);

            var splitted = Full.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Major = $"{splitted[0]}.{splitted[1]}";
        }

        public void AddBuildMetadata(string build, bool isDebug)
        {
            IsDebug = isDebug;
            Build = build;
        }

        public string Full => $"{_version}-{Build}";

        public string Build { get; private set; }
        public bool IsDebug { get; private set; }

        public string Major { get; }

        public DateTime BuildDate { get; }
    }
}
