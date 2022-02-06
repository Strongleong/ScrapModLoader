using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;

using Ionic.Zip;

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
        public String RequiredGame { get; private set; }
        public List<String> Authors { get; private set; }
        public Dictionary<String, List<String>> Credits { get; private set; }

        public ScrapMod(String path)
        {
            ModPath = path;
            Name = Path.GetFileNameWithoutExtension(path);
            Description = String.Empty;
            Icon = new BitmapImage();
            Checked = false;
            Category = String.Empty;
            Version = String.Empty;
            RequiredLauncher = String.Empty;
            RequiredGame = String.Empty;
            Authors = new List<String>();
            Credits = new Dictionary<String, List<String>>();
            LoadFromFile(path);
        }

        public void LoadModToGame(String gamePath)
        {
            gamePath += @"Mods\" + Name;
            Directory.CreateDirectory(gamePath);

            using (ZipFile zipFile = ZipFile.Read(ModPath))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (Path.GetExtension(zipEntry.FileName) == ".packed")
                    {
                        zipEntry.Extract(gamePath);
                    }
                }
            }
        }

        private void LoadFromFile(String path)
        {
            using (ZipFile zipFile = ZipFile.Read(path))
            {
                Byte[] iconBuffer = ExtractFromZip(zipFile, "icon.png");
                Byte[] confBuffer = ExtractFromZip(zipFile, "config.xml");
                LoadIcon(iconBuffer);
                LoadConfig(confBuffer);
            }
        }

        private Byte[] ExtractFromZip(ZipFile zip, String entry_path)
        {
            ZipEntry? entry = zip[entry_path];
            if (entry == null)
                throw new FileFormatException($"No '{entry}' in {Name} found");

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
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(sourceStream);
                ParseModInfoFromXml(xmlDocument);
                ParseAuthorsFromXml(xmlDocument);
                ParseCreditsFromXml(xmlDocument);
            }
        }

        private void ParseModInfoFromXml(XmlDocument xmlDocument)
        {
            Description = xmlDocument.GetElementsByTagName("Description").Item(0)?.InnerText ??
                                    throw new FileFormatException("No 'Description' tag in 'config.xml'");
            Category = xmlDocument.GetElementsByTagName("Category").Item(0)?.InnerText ??
                                    throw new FileFormatException("No 'Category' tag in 'config.xml'");
            Version = xmlDocument.GetElementsByTagName("Version").Item(0)?.InnerText ??
                                    throw new FileFormatException("No 'Version' tag in 'config.xml'");
            RequiredLauncher = xmlDocument.GetElementsByTagName("RequiredLauncher").Item(0)?.InnerText ??
                                    throw new FileFormatException("No 'RequiredLauncher' tag in 'config.xml'");
            RequiredGame = xmlDocument.GetElementsByTagName("RequiredGame").Item(0)?.InnerText ??
                                    throw new FileFormatException("No 'RequiredGame' tag in 'config.xml'");
        }

        private void ParseAuthorsFromXml(XmlDocument xmlDocument)
        {
            XmlNodeList authors = xmlDocument.GetElementsByTagName("Author");
            foreach (XmlNode author in authors)
            {
                XmlAttribute? nameAttr = author.Attributes?["name"];
                if (nameAttr == null)
                    throw new FileFormatException("No 'name' attribute in 'Author' tag in 'config.xml'");
                Authors.Add(nameAttr.InnerText);
            }
        }

        private void ParseCreditsFromXml(XmlDocument xmlDocument)
        {
            XmlNodeList credits = xmlDocument.GetElementsByTagName("Credits");
            foreach (XmlNode credit in credits)
            {
                List<String> entries = new List<String>();

                XmlAttribute? groupAttr = credit.Attributes?["group"];
                if (groupAttr == null)
                    throw new FileFormatException("No 'group' attribute in 'Credits' tag in 'config.xml'");

                foreach (XmlNode entry in credit)
                {
                    XmlAttribute? nameAttr = entry.Attributes?["name"];
                    if (nameAttr == null)
                        throw new FileFormatException("No 'name' attribute in 'Author' tag in 'config.xml'");

                    entries.Add(nameAttr.InnerText);
                }

                Credits.Add(groupAttr.InnerText, entries);
            }
        }
    }
}
