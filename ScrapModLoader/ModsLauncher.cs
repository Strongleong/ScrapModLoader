using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Win32;

namespace ScrapModLoader
{
    public class ModsLauncher
    {
        public List<ScrapMod> Mods { get; private set; }
        public String ScraplandPath { get; set; }
        public String ScraplandRemasteredPath { get; set; }

        public ModsLauncher()
        {
            Mods = new List<ScrapMod>();
            ScraplandPath = Settings.Default.ScraplandPath;
            ScraplandRemasteredPath = Settings.Default.ScraplandRemasteredPath;
        }

        public void ScanMods()
        {
            Mods.Clear();
            foreach (String? folder in Settings.Default.ModsPathes)
            {
                if (folder != null)
                {
                    String[] files = Directory.GetFiles(folder, "*.sm", SearchOption.AllDirectories);
                    foreach (String file in files)
                        Mods.Add(new ScrapMod(file));
                }
            }
        }

        public Boolean SearchForScrapland()
        {
            Boolean isFound = false;

            String[] registryPathes =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (String registryPath in registryPathes)
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key == null)
                    continue;

                foreach (String subkey_name in key.GetSubKeyNames())
                {
                    using RegistryKey? subkey = key.OpenSubKey(subkey_name);
                    if (subkey == null)
                        continue;

                    String? displayName = subkey.GetValue("DisplayName")?.ToString();
                    if (displayName == null)
                        continue;

                    if (displayName == "Scrapland")
                    {
                        ScraplandPath = subkey.GetValue("InstallLocation")?.ToString() ?? "";
                        isFound = true;
                    }

                    if (displayName == "Scrapland Remastered")
                    {
                        ScraplandRemasteredPath = subkey.GetValue("InstallLocation")?.ToString() ?? "";
                        isFound = true;
                    }
                }
            }

            return isFound;
        }
    }
}
