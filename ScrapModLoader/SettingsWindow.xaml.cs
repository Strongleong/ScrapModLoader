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
        
        public Boolean Save { get; set; }
        private ModsLauncher ModsLauncherInstance { get; set; }

        public SettingsWindow(ModsLauncher modsLauncher)
        {
            InitializeComponent();

            Save = false;
            ModsLauncherInstance = modsLauncher;

            ModsPathesList.ItemsSource = ModsLauncherInstance.ModsPathes;
            ScraplandPathTextBox.Text = ModsLauncherInstance.ScraplandPath;
            ScraplandRemasteredPathTextBox.Text = ModsLauncherInstance.ScraplandRemasteredPath;

            Boolean enable = ModsLauncherInstance.ScraplandPath != String.Empty;
            ButtonShowScrap.IsEnabled = enable;
            ButtonClearScrap.IsEnabled = enable;

            enable = ModsLauncherInstance.ScraplandRemasteredPath != String.Empty;
            ButtonShowScrapRemaster.IsEnabled = enable;
            ButtonClearScrapRemaster.IsEnabled = enable;
        }

        // -------------------------------------------
        // Mods Pathes
        // -------------------------------------------

        // Left buttons handlers
        private void ButtonAdd_Click(Object sender, RoutedEventArgs e)
        {
            String folder = Utils.GetFolderDialog();
            if (folder != String.Empty)
                ModsLauncherInstance.ModsPathes.Add(folder);

            ModsPathesList.Items.Refresh();
        }
        private void ButtonRemove_Click(Object sender, RoutedEventArgs e)
        {
            ModsLauncherInstance.ModsPathes.Remove((String)ModsPathesList.SelectedValue);
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

            String? temp = ModsLauncherInstance.ModsPathes[index - 1];
            ModsLauncherInstance.ModsPathes[index - 1] = ModsLauncherInstance.ModsPathes[index];
            ModsLauncherInstance.ModsPathes[index] = temp;

            ModsPathesList.Items.Refresh();
        }
        private void ButtonDown_Click(Object sender, RoutedEventArgs e)
        {
            Int32 index = ModsPathesList.SelectedIndex;
            if (index == ModsLauncherInstance.ModsPathes.Count - 1)
                return;

            String? temp = ModsLauncherInstance.ModsPathes[index + 1];
            ModsLauncherInstance.ModsPathes[index + 1] = ModsLauncherInstance.ModsPathes[index];
            ModsLauncherInstance.ModsPathes[index] = temp;

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
            String ScraplandPath = Utils.GetFolderDialog();
            if (ScraplandPath != String.Empty)
            {
                ScraplandPathTextBox.Text = ScraplandPath + "\\";
                ModsLauncherInstance.ScraplandPath = ScraplandPath + "\\";
                ButtonShowScrap.IsEnabled = true;
            }
        }
        private void ButtonBrowseScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            String ScraplandRemasteredPath = Utils.GetFolderDialog();
            if (ScraplandRemasteredPath != String.Empty)
            {
                ScraplandRemasteredPathTextBox.Text = ScraplandRemasteredPath + "\\";
                ModsLauncherInstance.ScraplandRemasteredPath = ScraplandRemasteredPath + "\\";
                ButtonShowScrapRemaster.IsEnabled = true;
            }
        }
        private void ButtonClearScrap_Click(Object sender, RoutedEventArgs e)
        {
            ScraplandPathTextBox.Text = String.Empty;
            ButtonClearScrap.IsEnabled = false;
            ButtonShowScrap.IsEnabled = false;
            ModsLauncherInstance.ScraplandPath = String.Empty;
        }
        private void ButtonClearScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            ScraplandRemasteredPathTextBox.Text = String.Empty;
            ButtonClearScrapRemaster.IsEnabled = false;
            ButtonShowScrapRemaster.IsEnabled = false;
            ModsLauncherInstance.ScraplandRemasteredPath = String.Empty;
        }
        private void ButtonShowScrap_Click(Object sender, RoutedEventArgs e)
        {
            String? path = Path.GetDirectoryName(ModsLauncherInstance.ScraplandPath);
            if (path == null)
                throw new DirectoryNotFoundException("Cannot find direcotry for Scrapland");
            Process.Start("explorer.exe", path);
        }
        private void ButtonShowScrapRemaster_Click(Object sender, RoutedEventArgs e)
        {
            String? path = Path.GetDirectoryName(ModsLauncherInstance.ScraplandRemasteredPath);
            if (path == null)
                throw new DirectoryNotFoundException("Cannot find direcotry for Scrapland");
            Process.Start("explorer.exe", path);
        }
        private void ButtonAutoFind_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                Boolean isFoundScrapland = ModsLauncherInstance.SearchForScrapland();
                if (!isFoundScrapland)
                    MessageBox.Show("Error: unable to find Scrapland instalation. Please, specify yours game installation folder in settings.");
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
            }

            ScraplandPathTextBox.Text = ModsLauncherInstance.ScraplandPath;
            ScraplandRemasteredPathTextBox.Text = ModsLauncherInstance.ScraplandRemasteredPath;

            Boolean enable = ModsLauncherInstance.ScraplandPath != String.Empty;
            ButtonShowScrap.IsEnabled = enable;
            ButtonClearScrap.IsEnabled = enable;

            enable = ModsLauncherInstance.ScraplandRemasteredPath != String.Empty;
            ButtonShowScrapRemaster.IsEnabled = enable;
            ButtonClearScrapRemaster.IsEnabled = enable;
        }

        // -------------------------------------------
        //  Window contols buttons
        // -------------------------------------------
        private void ButtonCancel_Click(Object sender, RoutedEventArgs e) =>
            Close();
        private void ButtonSave_Click(Object sender, RoutedEventArgs e)
        {
            Settings.Default.ModsPathes.Clear();
            Settings.Default.ModsPathes.AddRange(ModsLauncherInstance.ModsPathes.ToArray());
            Settings.Default.ScraplandPath = ModsLauncherInstance.ScraplandPath;
            Settings.Default.ScraplandRemasteredPath = ModsLauncherInstance.ScraplandRemasteredPath;
            Settings.Default.Save();

            Save = true;
            Close();
        }
    }
}
