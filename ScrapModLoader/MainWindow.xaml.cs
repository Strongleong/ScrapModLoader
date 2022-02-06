using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ScrapModLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ModsLauncher modsLauncher;

        private GridLength gridLength = new GridLength(1, GridUnitType.Star);

        public MainWindow()
        {
            modsLauncher = new ModsLauncher();
            InitializeComponent();
        }

        private void Window_Initialized(Object sender, EventArgs e)
        {
            if (Settings.Default.ModsPathes.Count == 0)
            {
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + Path.DirectorySeparatorChar + "Scrapland mods";
                Settings.Default.ModsPathes.Add(path);
                Directory.CreateDirectory(path);
            }

            // TODO: Refactor it to separate window with pretty loading animation
            if (modsLauncher.ScraplandPath == String.Empty && modsLauncher.ScraplandRemasteredPath == String.Empty)
            {
                Boolean isFoundScrapland = modsLauncher.SearchForScrapland();
                if (!isFoundScrapland)
                {
                    ButtonRunScrapland.IsEnabled = false;
                    MessageBox.Show("Error: unable to find Scrapland instalation. Please, specify yours game installation folder in settings.");
                }
            }

            ((ComboBoxItem)ScraplandVersion.Items[0]).IsEnabled = modsLauncher.ScraplandPath != String.Empty;
            ((ComboBoxItem)ScraplandVersion.Items[1]).IsEnabled = modsLauncher.ScraplandRemasteredPath != String.Empty;

            ScraplandVersion.SelectedIndex = modsLauncher.ScraplandRemasteredPath != String.Empty ? 1 : 0;

            modsLauncher.ScanMods();
        }

        private void ModsList_Initialized(Object sender, EventArgs e) => ModsList.ItemsSource = modsLauncher.Mods;

        private void ModsList_MouseDown(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MainGrid.ColumnDefinitions[2].Width.Value != 0)
            {
                gridLength = MainGrid.ColumnDefinitions[2].Width;
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Star);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            MainGrid.ColumnDefinitions[2].Width = gridLength;

            if (sender is ListViewItem item)
            {
                String? selectedModName = (item.Content as ScrapMod)?.Name;
                if (selectedModName == null)
                    throw new KeyNotFoundException(nameof(selectedModName));

                ScrapMod ? mod = modsLauncher.Mods.Find(mod => mod.Name == selectedModName);

                if (mod == null)
                    throw new KeyNotFoundException(nameof(mod));

                if (e.ClickCount == 2)
                {
                    mod.Checked = !mod.Checked;
                    ModsList.Items.Refresh();
                }

                WriteModInfo(mod);
            }
        }

        private void WriteModInfo(ScrapMod mod)
        {
            ModInfo.Document.Blocks.Clear();
            Paragraph parahraph = new Paragraph();

            parahraph.Inlines.Add(new Bold(new Run("Description:\n")));
            parahraph.Inlines.Add(new Run(mod.Description));

            parahraph.Inlines.Add(new Bold(new Run("\n\nAuthors:\n")));
            foreach (String autor in mod.Authors)
                parahraph.Inlines.Add(new Run(autor + "\n"));

            ModInfo.Document.Blocks.Add(parahraph);

            ModCreditsTab.Visibility = Visibility.Visible;
            if (mod.Credits.Count == 0)
                ModCreditsTab.Visibility = Visibility.Hidden;
            else
            {
                ModCredits.Document.Blocks.Clear();
                parahraph = new Paragraph();

                foreach (KeyValuePair<String, List<String>> credit in mod.Credits)
                {
                    parahraph.Inlines.Add(new Bold(new Run(credit.Key + "\n")));
                    foreach (String autor in credit.Value)
                        parahraph.Inlines.Add(new Run(autor + "\n"));
                    parahraph.Inlines.Add(new Run("\n"));
                }

                ModCredits.Document.Blocks.Add(parahraph);
            }
        }

        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            Boolean? isChecked = checkbox.IsChecked;

            if (isChecked == null)
                throw new NullReferenceException(nameof(isChecked));

            StackPanel parent = (StackPanel)checkbox.Parent;
            Label label = (Label)parent.Children[2];
            String? selectedModName = label.Content.ToString();

            if (selectedModName == null)
                throw new KeyNotFoundException(nameof(selectedModName));

            ScrapMod? mod = modsLauncher.Mods.Find(mod => mod.Name == selectedModName);

            if (mod == null)
                throw new KeyNotFoundException(nameof(selectedModName));

            mod.Checked = (Boolean)isChecked;
        }

        private void ButtonSettings_Click(Object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            if (settingsWindow.Save)
                modsLauncher.ScanMods();
            ModsList.Items.Refresh();
        }
    }
}
