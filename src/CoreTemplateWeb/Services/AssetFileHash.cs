using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CoreTemplateWeb.Services {
    public class AssetFileHash {
        private readonly IHostingEnvironment _env;
        public AssetFileHash(IHostingEnvironment env) {
            _env = env;
        }

        /// <summary>
        /// Converts file to hash for cache-busting. Depends on assets being in the WebRootPath (wwwroot by default).
        /// </summary>
        /// <param name="fileName">Must include any directory below wwwroot, e.g. 'js/app.js', 'css/app.css'</param>
        /// <returns>Hash of file</returns>
        public string GetForFile(string fileName) {
            try {
                // http://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
                using (var md5 = MD5.Create()) {
                    using (var stream = File.OpenRead($"{_env.WebRootPath}/{fileName}")) {
                        var bytes = md5.ComputeHash(stream);
                        // Standard-like string, e.g. 30ce58add34190c3332bc1cb197a9407
                        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                    }
                }
            } catch {
                return $"1111";
            }
        }

        public string LinkWithHash(string fileName) {
            return $"{fileName}?v={GetForFile(fileName)}";
        }
    }
}
