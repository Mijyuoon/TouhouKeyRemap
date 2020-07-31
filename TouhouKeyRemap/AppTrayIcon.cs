using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TouhouKeyRemap {
    class AppTrayIcon {
        private readonly Config.ConfigData _config;

        public AppTrayIcon(Config.ConfigData config) {
            _config = config;
        }

        public void Display() {
            var trayIcon = new NotifyIcon {
                Text = "Key Remapper Utility",
                Icon = Properties.Resources.appicon,
            };

            var menu = new ContextMenu();
            trayIcon.ContextMenu = menu;

            menu.MenuItems.Add("Exit", (s, e) => Application.Exit());

            trayIcon.Visible = true;
        }
    }
}
