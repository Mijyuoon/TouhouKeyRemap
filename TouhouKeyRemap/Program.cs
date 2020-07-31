using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TouhouKeyRemap {
    class Program {
        static void Main(string[] args) {
            Config.ConfigData config;

            using(var stream = new FileStream("keyremap.txt", FileMode.Open)) {
                var reader = new Config.ConfigReader(stream);
                if(!reader.TryParse(out config)) {
                    MessageBox.Show(
                        $"Failed to parse config file:\n{reader.ErrorMessage}",
                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
            }

            var processor = new KeyProcessor(config);
            var trayIcon = new AppTrayIcon(config);

            processor.Initialize();
            trayIcon.Display();

            Application.Run();
            processor.Shutdown();
        }
    }
}
