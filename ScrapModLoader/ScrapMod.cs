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
        public String Name { get; private set; }
        public String Description { get; private set; }
        public String ModPath { get; private set; }
        public BitmapImage Icon { get; private set; }
        public Boolean Checked { get; set; }
        public String Category { get; private set; }
        public String Version { get; private set; }
        public String RequiredLauncher { get; private set; }
        public List<String> SupportedGameVersions { get; private set; }
        public String SupportedGameVersionsDisplay { 
            get
            {
                String result = String.Empty;
                foreach (String version in SupportedGameVersions)
                    result += version + ", ";
                return result.TrimEnd(',', ' ');
            } 
        }
        public List<String> Authors { get; private set; }
        public Dictionary<String, List<String>> Credits { get; private set; }

        public ScrapMod(String path)
        {
            ModPath = path;
            Name = String.Empty;
            Description = String.Empty;
            Icon = new BitmapImage();
            Checked = false;
            Category = String.Empty;
            Version = String.Empty;
            RequiredLauncher = String.Empty;
            SupportedGameVersions = new List<String>();
            Authors = new List<String>();
            Credits = new Dictionary<String, List<String>>();
            LoadFromFile(path);
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

            using (ZipFile zipFile = ZipFile.Read(ModPath))
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

        private void LoadFromFile(String path)
        {
            using (ZipFile zipFile = ZipFile.Read(path))
            {
                Byte[] iconBuffer = ExtractFromZip(zipFile, "icon.png");
                Byte[] confBuffer = ExtractFromZip(zipFile, "config.toml");
                LoadIcon(iconBuffer);
                LoadConfig(confBuffer);
            }
        }

        private Byte[] ExtractFromZip(ZipFile zip, String entry_path)
        {
            ZipEntry? entry = zip[entry_path];
            if (entry == null)
                throw new FileFormatException($"No '{entry_path}' in {Name} found");

            Byte[] buffer = new Byte[entry.UncompressedSize];
            using (MemoryStream zipStream = new MemoryStream(buffer))
                entry.Extract(zipStream);

            return buffer;
        }

        private void LoadIcon(Byte[] buffer)
        {
            using (MemoryStream sourceStream = new MemoryStream(buffer))
            {
                Icon.BeginInit();
                Icon.CacheOption = BitmapCacheOption.OnLoad;
                Icon.StreamSource = sourceStream;
                Icon.EndInit();
            }
        }

        private void LoadConfig(Byte[] buffer)
        {
            using (MemoryStream sourceStream = new MemoryStream(buffer))
            using (StreamReader reader = new StreamReader(sourceStream))
            {
                TomlTable config = TOML.Parse(reader);

                CheckConfig(config);

                Name = config["title"];
                Description = config["description"];
                Category = config["category"];
                Version = config["version"];
                RequiredLauncher = config["requiredLauncher"];

                foreach (TomlNode version in config["supportedGameVersions"])
                    SupportedGameVersions.Add(version);

                foreach (TomlNode author in config["authors"])
                    Authors.Add(author["name"]);

                foreach (TomlNode credit in config["credits"])
                {
                    List<String> entries = new List<String>();

                    foreach (TomlNode entry in credit["credits"])
                        entries.Add(entry["name"]);

                    Credits.Add(credit["group"], entries);
                }
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
