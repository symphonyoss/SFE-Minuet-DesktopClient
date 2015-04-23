using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Paragon.Plugins.Notifications
{
    public class Monitors : IMonitors
    {
        public IEnumerable<IMonitor> AllMonitors()
        {
            return Screen.AllScreens
                .Select(screen => new Monitor(screen))
                .OfType<IMonitor>()
                .ToList();
        }
    }
}