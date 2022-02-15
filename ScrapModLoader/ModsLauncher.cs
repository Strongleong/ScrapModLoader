using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Ionic.Zip;

using Microsoft.Win32;

namespace ScrapModLoader
{
    public class ModsLauncher
    {
        public List<ScrapMod> Mods { get; private set; }
        public List<String> ModsPathes { get; set; }
        public String ScraplandPath { get; set; }
        public String ScraplandRemasteredPath { get; set; }
        public String SelectedGameVersion { get; set; }
        public String LauncherVersion { get; set; }
        public String SelectedGamePath { get; set; }

        public ModsLauncher()
        {
            Mods = new List<ScrapMod>();
            ModsPathes = Utils.StringCollectionToList(Settings.Default.ModsPathes);
            ScraplandPath = Settings.Default.ScraplandPath;
            ScraplandRemasteredPath = Settings.Default.ScraplandRemasteredPath;
            SelectedGameVersion = "0.0";
            LauncherVersion = "0.3";
            SelectedGamePath = String.Empty;
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

                    if (displayName == "Scrapland" || displayName == "American McGee presents Scrapland")
                    {
                        ScraplandPath = subkey.GetValue("InstallLocation")?.ToString() ?? "";
                        if (ScraplandPath == null || ScraplandPath == String.Empty)
                            throw new KeyNotFoundException("Installed Scrapland found, but unable to locate the instalation folder");

                        Settings.Default.ScraplandPath = ScraplandPath;
                        isFound = true;
                    }

                    if (displayName == "Scrapland Remastered")
                    {
                        ScraplandRemasteredPath = subkey.GetValue("InstallLocation")?.ToString() ?? "";
                        if (ScraplandRemasteredPath == null || ScraplandRemasteredPath == String.Empty)
                            throw new KeyNotFoundException("Installed Scrapland Remastered found, but unable to locate the instalation folder");

                        Settings.Default.ScraplandRemasteredPath = ScraplandRemasteredPath;
                        isFound = true;
                    }
                }
            }

            return isFound;
        }

        public void LoadMods()
        {
            SelectedGamePath = SelectedGameVersion == "1.0" ? ScraplandPath : ScraplandRemasteredPath;

            foreach (ScrapMod mod in Mods)
            {
                // TODO: Warning about not loading mods that not supports selected version
                if (!mod.SupportedGameVersions.Contains(SelectedGameVersion) ||
                    Single.Parse(mod.RequiredLauncher, CultureInfo.InvariantCulture) < Single.Parse(LauncherVersion, CultureInfo.InvariantCulture))
                    continue;

                if (mod.Checked)
                {
                    if (!IsEnabled(mod))
                        Enable(mod);
                }
                else
                {
                    if (IsEnabled(mod))
                        Disable(mod);
                }
            }
        }

        public Boolean IsLoaded(ScrapMod mod) =>
            Directory.Exists(SelectedGamePath + @"Mods\" + mod.Name);

        public Boolean IsEnabled(ScrapMod mod)
        {
            if (!IsLoaded(mod))
                return false;

            foreach (String file in Directory.EnumerateFiles(SelectedGamePath + @"Mods\" + mod.Name, "*.disabled", SearchOption.AllDirectories))
                return false;

            return true;
        }

        public void Enable(ScrapMod mod)
        {
            if (!IsLoaded(mod))
                LoadModToGame(mod);

            if (IsEnabled(mod))
                return;

            foreach (String file in Directory.EnumerateFiles(SelectedGamePath + @"Mods\" + mod.Name, "*.disabled", SearchOption.AllDirectories))
                File.Move(file, Path.ChangeExtension(file, null));
        }

        public void Disable(ScrapMod mod)
        {
            if (!IsEnabled(mod))
                return;

            foreach (String file in Directory.EnumerateFiles(SelectedGamePath + @"Mods\" + mod.Name, "*.packed", SearchOption.AllDirectories))
                File.Move(file, file + ".disabled");
        }

        private void LoadModToGame(ScrapMod mod)
        {
            String modPath = SelectedGamePath + @"Mods\" + mod.Name;
            Directory.CreateDirectory(modPath);

            using (ZipFile zipFile = ZipFile.Read(mod.ModPath))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!Path.GetFullPath(zipEntry.FileName).Contains(SelectedGameVersion))
                        continue;

                    if (Path.GetExtension(zipEntry.FileName) == ".packed")
                        zipEntry.Extract(modPath);
                }
            }
        }
    }
}
