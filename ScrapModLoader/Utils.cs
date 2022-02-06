using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

using Microsoft.Win32;

namespace ScrapModLoader
{
    internal static class Utils
    {
        public static String GetFilePath()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Scrapland Executable File (*.exe)|*.exe"
            };

            Nullable<Boolean> result = dlg.ShowDialog();
            return dlg.FileName;
        }
        
        public static String GetFolderDialog()
        {
            using System.Windows.Forms.FolderBrowserDialog? dialog = new System.Windows.Forms.FolderBrowserDialog();
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
    }
}
