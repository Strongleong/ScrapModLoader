using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Ionic.Zip;

using Microsoft.Win32;

namespace ScrapModLoader
{
    public class ModsLauncher
    {
        public List<ScrapMod> Mods { get; private set; } = new List<ScrapMod>();
        public List<String> ModsPathes { get; set; } = Utils.StringCollectionToList(Settings.Default.ModsPathes);
        public String ScraplandPath { get; set; } = Settings.Default.ScraplandPath;
        public String ScraplandRemasteredPath { get; set; } = Settings.Default.ScraplandRemasteredPath;
        public String SelectedGameVersion { get; set; } = "0.0";
        public String LauncherVersion { get; set; } = "0.3";
        public String SelectedGamePath { get; set; } = String.Empty;

        public delegate void ModsLoadedHandler(ModLoadedEventArgs eventArgs);

        public event ModsLoadedHandler? ModsLoaded;

        public ModsLauncher()
        {

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

                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    using RegistryKey? subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null)
                        continue;

                    String? displayName = subKey.GetValue("DisplayName")?.ToString();
                    if (displayName == null)
                        continue;

                    if (displayName == "Scrapland" || displayName == "American McGee presents Scrapland")
                    {
                        ScraplandPath = subKey.GetValue("InstallLocation")?.ToString() ?? String.Empty;
                        if (String.IsNullOrEmpty(ScraplandPath))
                            throw new KeyNotFoundException("Installed Scrapland found, but unable to locate the instalation folder");

                        Settings.Default.ScraplandPath = ScraplandPath;
                        isFound = true;
                    }

                    if (displayName == "Scrapland Remastered")
                    {
                        ScraplandRemasteredPath = subKey.GetValue("InstallLocation")?.ToString() ?? String.Empty;
                        if (String.IsNullOrEmpty(ScraplandRemasteredPath))
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
            List<ScrapMod> loadedMods = new List<ScrapMod>();
            List<ScrapMod> unsupportedMods = new List<ScrapMod>();

            foreach (ScrapMod mod in Mods)
            {
                if (IsSupported(mod))
                {
                    if (mod.Checked)
                    {
                        if (!IsEnabled(mod))
                            Enable(mod);

                        loadedMods.Add(mod);
                    }
                    else
                    {
                        if (IsEnabled(mod))
                            Disable(mod);
                    }
                }
                else
                {
                    unsupportedMods.Add(mod);
                }
            }
            ModsLoaded?.Invoke(new ModLoadedEventArgs(loadedMods, unsupportedMods));
        }

        public Boolean IsSupported(ScrapMod mod)
        {
            return mod.SupportedGameVersions.Contains(SelectedGameVersion) ||
                    Single.Parse(mod.RequiredLauncher, CultureInfo.InvariantCulture) < Single.Parse(LauncherVersion, CultureInfo.InvariantCulture);
        }

        private String ModPath(ScrapMod mod) => 
            Path.Combine(SelectedGamePath, "Mods", mod.Name);

        public Boolean IsLoaded(ScrapMod mod) =>
            Directory.Exists(ModPath(mod));

        public Boolean IsEnabled(ScrapMod mod)
        {
            if (!IsLoaded(mod))
                return false;

            return Directory.EnumerateFiles(ModPath(mod), "*.disabled", SearchOption.AllDirectories).FirstOrDefault() == null;
        }

        public void Enable(ScrapMod mod)
        {
            if (!IsLoaded(mod))
                LoadModToGame(mod);

            if (IsEnabled(mod))
                return;

            foreach (String file in Directory.EnumerateFiles(ModPath(mod), "*.disabled", SearchOption.AllDirectories))
                File.Move(file, Path.ChangeExtension(file, null));
        }

        public void Disable(ScrapMod mod)
        {
            if (!IsEnabled(mod))
                return;

            foreach (String file in Directory.EnumerateFiles(ModPath(mod), "*.packed", SearchOption.AllDirectories))
                File.Move(file, file + ".disabled");
        }

        private void LoadModToGame(ScrapMod mod)
        {
            Directory.CreateDirectory(ModPath(mod));

            using (ZipFile zipFile = ZipFile.Read(mod.ModPath))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!Path.GetFullPath(zipEntry.FileName).Contains(SelectedGameVersion))
                        continue;

                    if (Path.GetExtension(zipEntry.FileName) == ".packed")
                        zipEntry.Extract(ModPath(mod));
                }
            }
        }
    }
}
