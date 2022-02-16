using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Text;

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
            modsLauncher.ModsLoaded += ModsLauncher_ModsLoaded;
            InitializeComponent();
        }

        private void Window_Initialized(Object sender, EventArgs e)
        {
            if (Settings.Default.ModsPathes.Count == 0)
            {
                String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scrapland mods");
                Settings.Default.ModsPathes.Add(path);
                Directory.CreateDirectory(path);
            }

            // TODO: Refactor it to separate window with pretty loading animation
            if (modsLauncher.ScraplandPath == String.Empty && modsLauncher.ScraplandRemasteredPath == String.Empty)
            {
                try
                {
                    Boolean isFoundScrapland = modsLauncher.SearchForScrapland();
                    if (!isFoundScrapland)
                    {
                        ButtonRunScrapland.IsEnabled = false;
                        MessageBox.Show("Unable to find Scrapland instalation. Please, specify yours game installation folder in settings.",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            OriginalVersionItem.IsEnabled = modsLauncher.ScraplandPath != String.Empty;
            RemasteredVersionItem.IsEnabled = modsLauncher.ScraplandRemasteredPath != String.Empty;

            ScraplandVersion.SelectedIndex = modsLauncher.ScraplandRemasteredPath != String.Empty ? 1 : 0;

            modsLauncher.ScanMods();
        }

        private void ModsList_Initialized(Object sender, EventArgs e) => ModsList.ItemsSource = modsLauncher.Mods;

        private void ModsList_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (PreviewColumn.Width.Value != 0)
            {
                gridLength = PreviewColumn.Width;
                PreviewColumn.Width = new GridLength(0, GridUnitType.Star);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            PreviewColumn.Width = gridLength;

            if (sender is ListViewItem item)
            {
                String? selectedModName = (item.Content as ScrapMod)?.Name;
                if (selectedModName == null)
                    throw new KeyNotFoundException(nameof(selectedModName));

                ScrapMod? mod = modsLauncher.Mods.Find(mod => mod.Name == selectedModName);

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
            Paragraph paragraph = new Paragraph();

            paragraph.Inlines.Add(new Bold(new Run("Description:\n")));
            paragraph.Inlines.Add(new Run(mod.Description));

            paragraph.Inlines.Add(new Bold(new Run("\n\nAuthors:\n")));
            foreach (String autor in mod.Authors)
                paragraph.Inlines.Add(new Run(autor + "\n"));

            ModInfo.Document.Blocks.Add(paragraph);

            if (mod.Credits.Count == 0)
                ModCreditsTab.Visibility = Visibility.Hidden;
            else
            {
                ModCreditsTab.Visibility = Visibility.Visible;

                ModCredits.Document.Blocks.Clear();
                paragraph = new Paragraph();

                foreach (KeyValuePair<String, List<String>> credit in mod.Credits)
                {
                    paragraph.Inlines.Add(new Bold(new Run(credit.Key + "\n")));
                    foreach (String autor in credit.Value)
                        paragraph.Inlines.Add(new Run(autor + "\n"));
                    paragraph.Inlines.Add(new Run("\n"));
                }

                ModCredits.Document.Blocks.Add(paragraph);
            }
        }

        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            Boolean? isChecked = checkbox.IsChecked;

            if (isChecked == null)
                throw new NullReferenceException(nameof(isChecked));

            StackPanel parent = (StackPanel)checkbox.Parent;
            // TODO: replace by find template
            // https://docs.microsoft.com/ru-ru/dotnet/desktop/wpf/data/how-to-find-datatemplate-generated-elements?view=netframeworkdesktop-4.8
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
            SettingsWindow settingsWindow = new SettingsWindow(modsLauncher);

            settingsWindow.ShowDialog();
            if (settingsWindow.Save)
                modsLauncher.ScanMods();

            ModsList.Items.Refresh();
            OriginalVersionItem.IsEnabled = modsLauncher.ScraplandPath != String.Empty;
            RemasteredVersionItem.IsEnabled = modsLauncher.ScraplandRemasteredPath != String.Empty;
            ScraplandVersion.SelectedIndex = modsLauncher.ScraplandRemasteredPath != String.Empty ? 1 : 0;
        }

        private void ButtonRunScrapland_Click(Object sender, RoutedEventArgs e)
        {
            modsLauncher.LoadMods();

            String args = "-fullscreen:1";
            if (Windowed.IsChecked ?? false)
                args = "-fullscreen:0";

            String gamePath = modsLauncher.SelectedGameVersion == "1.0" ?
                modsLauncher.ScraplandPath : modsLauncher.ScraplandRemasteredPath;

            Process.Start(Path.Combine(gamePath, @"bin\Scrap.exe"), args);

            if (CloseLauncher.IsChecked ?? false)
                Close();
        }

        private void ModsLauncher_ModsLoaded(ModLoadedEventArgs eventArgs)
        {
            if (eventArgs.UnsupportedMods.Count != 0)
            {
                StringBuilder unsupportedModsBuilder = new StringBuilder();
                unsupportedModsBuilder.AppendLine("Next mod is unsupported and don't be loaded:");
                unsupportedModsBuilder.AppendLine();
                foreach (ScrapMod mod in eventArgs.UnsupportedMods)
                {
                    unsupportedModsBuilder.AppendLine(mod.Name);
                }

                MessageBox.Show(unsupportedModsBuilder.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ScraplandVersion_SelectionChanged(Object sender, SelectionChangedEventArgs e) =>
            modsLauncher.SelectedGameVersion = ScraplandVersion.SelectedIndex == 0 ? "1.0" : "1.1";
    }
}
