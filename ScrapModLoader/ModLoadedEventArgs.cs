using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapModLoader
{
    public class ModLoadedEventArgs : EventArgs
    {
        private List<ScrapMod> LoadedModsList { get; set; } = new List<ScrapMod>();
        private List<ScrapMod> UnsupportedModsList { get; set; } = new List<ScrapMod>();

        public ModLoadedEventArgs(IEnumerable<ScrapMod> loadedMods, IEnumerable<ScrapMod> unsupportedMods) : base()
        {
            LoadedModsList = loadedMods.ToList();
            UnsupportedModsList = unsupportedMods.ToList();
        }

        public ReadOnlyCollection<ScrapMod> LoadedMods => LoadedModsList.AsReadOnly();
        public ReadOnlyCollection<ScrapMod> UnsupportedMods => UnsupportedModsList.AsReadOnly();
    }
}
