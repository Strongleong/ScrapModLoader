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
        public String SelectedGameVersion { get; set; }
        public String LauncherVersion { get; set; }

        public ModsLauncher()
        {
            Mods = new List<ScrapMod>();
            ScraplandPath = Settings.Default.ScraplandPath;
            ScraplandRemasteredPath = Settings.Default.ScraplandRemasteredPath;
            SelectedGameVersion = "0.0";
            LauncherVersion = "0.3";
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
                        Mods.Add(ScrapMod.LoadFromFile(file));
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
                        Settings.Default.ScraplandPath = ScraplandPath;
                        isFound = true;
                    }

                    if (displayName == "Scrapland Remastered")
                    {
                        ScraplandRemasteredPath = subkey.GetValue("InstallLocation")?.ToString() ?? "";
                        Settings.Default.ScraplandRemasteredPath = ScraplandRemasteredPath;
                        isFound = true;
                    }
                }
            }

            return isFound;
        }

        public void LoadMods()
        {
            String gamePath = SelectedGameVersion == "1.0" ? ScraplandPath : ScraplandRemasteredPath;

            foreach (ScrapMod mod in Mods)
            {
                // TODO: Warning about not loading mods that not supports selected version
                if (!mod.SupportedGameVersions.Contains(SelectedGameVersion) ||
                    Single.Parse(mod.RequiredLauncher) < Single.Parse(LauncherVersion))
                    continue;

                if (mod.Checked)
                    if (!mod.IsEnabled(gamePath))
                        mod.Enable(gamePath, SelectedGameVersion);
                    else
                    if (mod.IsEnabled(gamePath))
                        mod.Disable(gamePath);
            }
        }
    }
}
