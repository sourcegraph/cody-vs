using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IStatusbarService
    {
        void SetText(string text);

        void StartProgressAnimation();

        void StopProgressAnimation();

        void SetCodyStatus(CodyStatus status, string tooltip = null);

        event EventHandler CodyStatusIconClicked;
    }

    public enum CodyStatus
    {
        Hide,
        Loading,
        Available,
        Unavailable
    }
}
