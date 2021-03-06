using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Media.Imaging;

using Ionic.Zip;


namespace ScrapModLoader;

internal static class Utils
{
    public static String GetFolderDialog()
    {
        using System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        return dialog.SelectedPath;
    }

    public static List<String> StringCollectionToList(StringCollection collection)
    {
        List<String> list = new List<String>();
        foreach (String? item in collection)
        {
            if (item != null)
                list.Add(item);
        }
        return list;
    }

    public static Byte[] ExtractFromZip(ZipFile zip, String entry_path)
    {
        ZipEntry? entry = zip[entry_path];
        if (entry == null)
            throw new FileFormatException($"No '{entry_path}' in {zip.Name} found");

        Byte[] buffer = new Byte[entry.UncompressedSize];
        using (MemoryStream zipStream = new MemoryStream(buffer))
            entry.Extract(zipStream);

        return buffer;
    }

    public static BitmapImage LoadImage(Byte[] buffer)
    {
        using MemoryStream sourceStream = new MemoryStream(buffer);

        BitmapImage? image = new BitmapImage();

        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = sourceStream;
        image.EndInit();

        return image;
    }
}

