using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ScrapModLoader
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public List<String> ModsPathes { get; set; }
        public String ScraplandPath { get; set; }
        public String ScraplandRemasteredPath { get; set; }
        public Boolean Save { get; set; }

        public SettingsWindow(ModsLauncher modsLauncher)
        {
            InitializeComponent();

            Save = false;

            ModsPathes = Utils.StringCollectionToList(Settings.Default.ModsPathes);
            ScraplandPath = Settings.Default.ScraplandPath;
            ScraplandRemasteredPath = Settings.Default.ScraplandRemasteredPath;

            ModsPathesList.ItemsSource = ModsPathes;
            ScraplandPathTextBox.Text = ScraplandPath;
            ScraplandRemasteredPathTextBox.Text = ScraplandRemasteredPath;

            ButtonShowScrap.IsEnabled = ScraplandPath != String.Empty;
            ButtonShowScrapRemaster.IsEnabled = ScraplandRemasteredPath != String.Empty;
        }

        // -------------------------------------------
        // Mods Pathes
        // -------------------------------------------

        // Left buttons handlers
        private void ButtonAdd_Click(Object sender, RoutedEventArgs e)
        {
            String folder = Utils.GetFolderDialog();
            if (folder != String.Empty)
                ModsPathes.Add(folder);

            ModsPathesList.Items.Refresh();
        }
        private void ButtonRemove_Click(Object sender, RoutedEventArgs e)
        {
            ModsPathes.Remove((String)ModsPathesList.SelectedValue);
            ModsPathesList.Items.Refresh();

            ButtonRemove.IsEnabled = false;
            ButtonUp.IsEnabled = false;
            ButtonDown.IsEnabled = false;
            ButtonOpen.IsEnabled = false;
        }
        private void ButtonUp_Click(Object sender, RoutedEventArgs e)
        {
            Int32 index = ModsPathesList.SelectedIndex;
            if (index == 0)
                return;

            String? temp = ModsPathes[index - 1];
            ModsPathes[index - 1] = ModsPathes[index];
            ModsPathes[index] = temp;

            ModsPathesList.Items.Refresh();
        }
        private void ButtonDown_Click(Object sender, RoutedEventArgs e)
        {
            Int32 index = ModsPathesList.SelectedIndex;
            if (index == ModsPathes.Count - 1)
                return;

            String? temp = ModsPathes[index + 1];
            ModsPathes[index + 1] = ModsPathes[index];
            ModsPathes[index] = temp;

            ModsPathesList.Items.Refresh();
        }
        private void ButtonOpen_Click(Object sender, RoutedEventArgs e) =>
            Process.Start("explorer.exe", (String)ModsPathesList.SelectedValue);

        // Left buttons controls
        private void ModsPathesList_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ButtonRemove.IsEnabled = true;
            ButtonUp.IsEnabled = true;
            ButtonDown.IsEnabled = true;
            ButtonOpen.IsEnabled = true;
        }
        private void ModsPathesList_PreviewMouseLeftButtonDown(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ModsPathesList.SelectedItem = null;
            ButtonRemove.IsEnabled = false;
            ButtonUp.IsEnabled = false;
            ButtonDown.IsEnabled = false;
            ButtonOpen.IsEnabled = false;
        }

        // -------------------------------------------
        //  Game executabls
        // -------------------------------------------

        private void ButtonBrowseScrap_Click(Object sender, RoutedEventArgs e)
        {
            String scraplandPath = Utils.GetFolderDialog();
            if (scraplandPath != String.Empty)
            {
                ScraplandPathTextBox.Text = scraplandPath;
                ScraplandPath = scraplandPath;
                ButtonShowScrap.IsEnabled = true;
            }
        }
        private void ButtonBrowseScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            String scraplandRemasteredPath = Utils.GetFilePath();
            if (scraplandRemasteredPath != String.Empty)
            {
                ScraplandRemasteredPathTextBox.Text = scraplandRemasteredPath;
                ScraplandRemasteredPath = scraplandRemasteredPath;
                ButtonShowScrapRemaster.IsEnabled = true;
            }
        }
        private void ButtonClearScrap_Click(Object sender, RoutedEventArgs e)
        {
            ScraplandPathTextBox.Text = String.Empty;
            ButtonShowScrap.IsEnabled = false;
            ScraplandPath = String.Empty;
        }
        private void ButtonClearScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            ScraplandRemasteredPathTextBox.Text = String.Empty;
            ButtonClearScrapRemaster.IsEnabled = false;
            ScraplandRemasteredPath = String.Empty;
        }
        private void ButtonShowScrap_Click(Object sender, RoutedEventArgs e)
        {
            String? path = Path.GetDirectoryName(ScraplandPath);
            if (path == null)
                throw new DirectoryNotFoundException("Cannot find direcotry for Scrapland");
            Process.Start("explorer.exe", path);
        }
        private void ButtonShowScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            String? path = Path.GetDirectoryName(ScraplandRemasteredPath);
            if (path == null)
                throw new DirectoryNotFoundException("Cannot find direcotry for Scrapland");
            Process.Start("explorer.exe", path);
        }

        // -------------------------------------------
        //  Window contols buttons
        // -------------------------------------------
        private void ButtonCancel_Click(Object sender, RoutedEventArgs e) =>
            Close();
        private void ButtonSave_Click(Object sender, RoutedEventArgs e)
        {
            Settings.Default.ModsPathes.Clear();
            Settings.Default.ModsPathes.AddRange(ModsPathes.ToArray());
            Settings.Default.ScraplandPath = ScraplandPath;
            Settings.Default.ScraplandRemasteredPath = ScraplandRemasteredPath;
            Settings.Default.Save();
            Save = true;
            Close();
        }
    }
}
