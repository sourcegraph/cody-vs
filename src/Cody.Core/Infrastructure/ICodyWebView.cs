using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Cody.Core.Infrastructure
{
    public interface ICodyWebView
    {
        event EventHandler<string> SendWebMessage;

        void PostWebMessage(string message);

        Task WaitUntilWebViewReady();

        Task ChangeColorTheme(string colorTheme);
    }
}
