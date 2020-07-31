using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TouhouKeyRemap.Config {
    #region Config Models

    struct RemapData {
        public uint Vk;
    }

    struct RescaleData {
        public int X;
        public int Y;
    }

    struct ConfigData {
        public ISet<string> EnableFor;
        public IDictionary<uint, RemapData> KeyRemap;
        public IDictionary<uint, RescaleData> KeyRescale;
    }

    #endregion

    class ConfigReader {
        private readonly char[] OuterSep = new[] { ' ', '\t' };
        private readonly char[] InnerSep = new[] { ',', ';' };

        private TextReader _reader;
        
        public string ErrorMessage { get; private set; }

        public ConfigReader(Stream inStream) {
            _reader = new StreamReader(inStream);
        }

        public bool TryParse(out ConfigData config) {
            config = new ConfigData {
                EnableFor = new HashSet<string>(),
                KeyRemap = new Dictionary<uint, RemapData>(),
            };

            string line;
            int lineNo = 1;

            bool SetError(string message) {
                ErrorMessage = $"[Line {lineNo}] {message}";
                return false;
            }

            for(; (line = _reader.ReadLine()) != null; lineNo++) {
                line = line.Trim();

                if(line[0] == '#') continue;
                if(string.IsNullOrWhiteSpace(line)) continue;

                int colonPos = line.IndexOf(':');
                if(colonPos < 0) return SetError("No parameter name found");

                string name = line.Substring(0, colonPos);
                string value = line.Substring(colonPos + 1);

                switch(name) {
                case "for":
                    config.EnableFor.UnionWith(value.Split(OuterSep, StringSplitOptions.RemoveEmptyEntries));
                    break;

                case "map": {
                        foreach(var entry in value.Split(OuterSep, StringSplitOptions.RemoveEmptyEntries)) {
                            var values = entry.Split(InnerSep, StringSplitOptions.None);
                            if(values.Length != 2) return SetError($"Invalid entry format: {entry}");

                            if(!ReadNumber(values[0], out uint num1))
                                return SetError($"Cannot parse number: {values[0]}");

                            if(!ReadNumber(values[1], out uint num2))
                                return SetError($"Cannot parse number: {values[1]}");

                            config.KeyRemap[num1] = new RemapData { Vk = num2 };
                        }
                    }
                    break;

                default:
                    return SetError($"Unknown parameter: {name}");
                }
            }

            return true;
        }

        private bool ReadNumber(string input, out uint result) {
            var format = NumberStyles.None;

            if(input[input.Length - 1] == 'h') {
                format |= NumberStyles.AllowHexSpecifier;
                input = input.Substring(0, input.Length - 1);
            }

            return uint.TryParse(input, format, CultureInfo.InvariantCulture, out result);
        }
    }
}
