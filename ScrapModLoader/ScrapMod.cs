using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

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

        public static ScrapMod LoadFromFile(String path)
        {
            using ZipFile zipFile = ZipFile.Read(path);

            Byte[] iconBuffer = Utils.ExtractFromZip(zipFile, "icon.png");
            Byte[] confBuffer = Utils.ExtractFromZip(zipFile, "config.toml");

            ScrapMod mod = new ScrapMod()
            {
                ModPath = path,
                Icon = Utils.LoadImage(iconBuffer)
            };

            LoadConfig(mod, confBuffer);

            return mod;
        }

        private static void LoadConfig(ScrapMod mod, Byte[] buffer)
        {
            using MemoryStream sourceStream = new MemoryStream(buffer);
            using StreamReader reader = new StreamReader(sourceStream);

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
