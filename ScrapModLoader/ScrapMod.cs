using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;

using Ionic.Zip;

using Tommy;

namespace ScrapModLoader
{
    public class ScrapMod
    {
        public String Name { get; private set; } = String.Empty;

        public String Description { get; private set; } = String.Empty;

        public String ModPath { get; private set; } = String.Empty;

        public BitmapImage Icon { get; private set; } = new BitmapImage();

        public Boolean Checked { get; set; } = false;

        public String Category { get; private set; } = String.Empty;

        public String Version { get; private set; } = String.Empty;

        public String RequiredLauncher { get; private set; } = String.Empty;

        public List<String> SupportedGameVersions { get; private set; } = new List<String>();

        public String SupportedGameVersionsDisplay => String.Join(", ", SupportedGameVersions);

        public List<String> Authors { get; private set; } = new List<String>();

        public Dictionary<String, List<String>> Credits { get; private set; } = new Dictionary<String, List<String>>();

        private ScrapMod()
        {

        }


        public Boolean IsLoaded(String gamePath) => Directory.Exists(gamePath + @"Mods\" + Name);

        public Boolean IsEnabled(String gamePath)
        {
            if (IsLoaded(gamePath))
            {
                foreach (String file in Directory.EnumerateFiles(gamePath + @"Mods\" + Name))
                {
                    if (Path.GetExtension(file) == ".disabled")
                        return false;
                }

                return true;
            }
            return false;
        }

        public void Enable(String gamePath, String gameVersion)
        {
            if (!IsLoaded(gamePath))
                LoadModToGame(gamePath, gameVersion);

            if (IsEnabled(gamePath))
                return;

            foreach (String file in Directory.EnumerateFiles(gamePath + @"Mods\" + Name))
                if (Path.GetExtension(file) == ".disabled")
                    File.Move(file, Path.ChangeExtension(file, null));
        }

        public void Disable(String gamePath)
        {
            if (!IsEnabled(gamePath))
                return;

            foreach (String file in Directory.EnumerateFiles(gamePath + @"Mods\" + Name))
                if (Path.GetExtension(file) == ".packed")
                    File.Move(file, file + ".disabled");
        }

        private void LoadModToGame(String gamePath, String gameVersion)
        {
            gamePath += @"Mods\" + Name;
            Directory.CreateDirectory(gamePath);

            using (var zipFile = ZipFile.Read(ModPath))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!Path.GetFullPath(zipEntry.FileName).Contains(gameVersion))
                        continue;

                    if (Path.GetExtension(zipEntry.FileName) == ".packed")
                        zipEntry.Extract(gamePath);
                }
            }
        }

        public static ScrapMod LoadFromFile(String path)
        {
            using var zipFile = ZipFile.Read(path);

            Byte[] iconBuffer = Utils.ExtractFromZip(zipFile, "icon.png");
            Byte[] confBuffer = Utils.ExtractFromZip(zipFile, "config.toml");

            var mod = new ScrapMod()
            {
                ModPath = path,
                Icon = Utils.LoadImage(iconBuffer)
            };

            LoadConfig(ref mod, confBuffer);

            return mod;
        }



        private static void LoadConfig(ref ScrapMod mod, Byte[] buffer)
        {
            using var sourceStream = new MemoryStream(buffer);
            using var reader = new StreamReader(sourceStream);

            TomlTable config = TOML.Parse(reader);

            CheckConfig(config);

            mod.Name = config["title"];
            mod.Description = config["description"];
            mod.Category = config["category"];
            mod.Version = config["version"];
            mod.RequiredLauncher = config["requiredLauncher"];

            foreach (TomlNode version in config["supportedGameVersions"])
                mod.SupportedGameVersions.Add(version);

            foreach (TomlNode author in config["authors"])
                mod.Authors.Add(author["name"]);

            foreach (TomlNode credit in config["credits"])
            {
                List<String> entries = new List<String>();

                foreach (TomlNode entry in credit["credits"])
                    entries.Add(entry["name"]);

                mod.Credits.Add(credit["group"], entries);
            }
        }

        private static void CheckConfig(TomlTable config)
        {
            if (!config.HasKey("title"))
                throw new FileFormatException("No 'title' key in 'config.toml'");

            if (!config.HasKey("description"))
                throw new FileFormatException("No 'description' key in 'config.toml'");

            if (!config.HasKey("category"))
                throw new FileFormatException("No 'category' key in 'config.toml'");

            if (!config.HasKey("version"))
                throw new FileFormatException("No 'version' key in 'config.toml'");

            if (!config.HasKey("requiredLauncher"))
                throw new FileFormatException("No 'name' key in 'config.toml'");

            if (!config.HasKey("supportedGameVersions"))
                throw new FileFormatException("No 'supportedGameVersions' key in 'config.toml'");
        }
    }
}
