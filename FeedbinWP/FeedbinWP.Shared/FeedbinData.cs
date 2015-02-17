using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace FeedbinWP
{
    class FeedbinData
    {
        public ObservableCollection<FeedbinEntry> allEntries { get; set; }
        public ObservableCollection<FeedbinEntry> unreadEntries { get; set; }
        public ObservableCollection<FeedbinEntry> starredEntries { get; set; }
        public ObservableCollection<FeedbinEntry> recentEntries { get; set; }

        public FeedbinData()
        {
            this.allEntries = new ObservableCollection<FeedbinEntry>();
            this.unreadEntries = new ObservableCollection<FeedbinEntry>();
            this.starredEntries = new ObservableCollection<FeedbinEntry>();
            this.recentEntries = new ObservableCollection<FeedbinEntry>();
        }
        
        public void addAllEntries(ObservableCollection<FeedbinEntry> all)
        {
            this.allEntries = all;
            this.unreadEntries = new ObservableCollection<FeedbinEntry>(allEntries.Where(a => a.read == false));
            this.starredEntries = new ObservableCollection<FeedbinEntry>(allEntries.Where(a => a.starred == true));
            this.recentEntries = new ObservableCollection<FeedbinEntry>(allEntries.Where(a => a.recent == true));
        }
    }
}
