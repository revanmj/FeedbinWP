using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FeedbinWP.Data;
using FeedbinWP.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FeedbinWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FeedbinData data;
        SettingsData settings;
        SQLiteAsyncConnection db;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            settings = new SettingsData();
            settings.readSettings();
            
            data = new FeedbinData();

            db = new SQLiteAsyncConnection("feedbinData.db");

            if (settings.loggedIn)
            {
                loadData();
                if (settings.syncAtStart)
                    syncData();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param> 

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            settings.readSettings();

            if (!settings.loggedIn)
            {
                SyncButton.IsEnabled = false;
                AddButton.IsEnabled = false;
                LogInButton.Visibility = Visibility.Visible;
            }
            else if (settings.loggedIn)
            {
                SyncButton.IsEnabled = true;
                AddButton.IsEnabled = true;
                LogInButton.Visibility = Visibility.Collapsed;

                if (!settings.syncRecent)
                    recentSection.Visibility = Visibility.Collapsed;
                if (!settings.syncStarred)
                    starredSection.Visibility = Visibility.Collapsed;

                loadData();
            }
        }

        private async void loadData()
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Loading data ...";
            await progressbar.ShowAsync();
            await db.CreateTableAsync<FeedbinEntry>();
            List<FeedbinEntry> starredEntries = new List<FeedbinEntry>();
            List<FeedbinEntry> recentEntries = new List<FeedbinEntry>();

            List<FeedbinEntry> unreadEntries = await db.Table<FeedbinEntry>().Where(x => x.read == false).ToListAsync();
            if (settings.syncStarred)
                starredEntries = await db.Table<FeedbinEntry>().Where(x => x.starred == true).ToListAsync();
            
            if (settings.syncRecent)
                recentEntries = await db.Table<FeedbinEntry>().Where(x => x.recent == true).ToListAsync();

            List<FeedbinEntry> allEntries = await db.Table<FeedbinEntry>().ToListAsync();

            if (unreadEntries != null)
                data.unreadEntries = new ObservableCollection<FeedbinEntry>(unreadEntries.OrderByDescending(f => f.published));
            if (starredEntries != null && settings.syncStarred)
                data.starredEntries = new ObservableCollection<FeedbinEntry>(starredEntries.OrderByDescending(f => f.published));
            if (recentEntries != null && settings.syncRecent)
                data.recentEntries = new ObservableCollection<FeedbinEntry>(recentEntries.OrderByDescending(f => f.published));
            if (allEntries != null)
                data.allEntries = new ObservableCollection<FeedbinEntry>(allEntries.OrderByDescending(f => f.published));

            this.DataContext = null;
            this.DataContext = data;

            await progressbar.HideAsync();
        }

        private async void syncData()
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Syncing ...";
            await progressbar.ShowAsync();

            await db.CreateTableAsync<FeedbinEntry>();

            int result = 0;
            await FeedbinSyncSqlite.resetReadStatus();

            if (settings.syncRead)
            {
                int r = await FeedbinSyncSqlite.getEntriesSince(DateTime.Now.AddDays(settings.daysToSync));
                if (r == 1)
                    result = 1;
            }
            if (settings.syncRecent)
            {
                int r = await FeedbinSyncSqlite.getRecentlyRead();
                if (r == 1)
                    result = 1;
            }
            if (settings.syncStarred)
            {
                int r = await FeedbinSyncSqlite.getStarredItems();
                if (r == 1)
                    result = 1;
            }

            int t = await FeedbinSyncSqlite.getUnreadItems();
            if (t == 1)
                result = 1;

            await FeedbinSyncSqlite.cleanupDatabase(200);

            await progressbar.HideAsync();

            if (result == 1)
                loadData();
        }

        private void Settings_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void LogIn_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void AddSubs_Click(Object sender, RoutedEventArgs e)
        {

        }

        private async void AllRead_Click(Object sender, RoutedEventArgs e)
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Marking all as read ...";
            await progressbar.ShowAsync();

            await FeedbinSyncSqlite.markAllAsRead();

            await progressbar.HideAsync();

        }

        private void Sync_Click(Object sender, RoutedEventArgs e)
        {
            syncData();
        }

        private void ArticleClick(Object sender, ItemClickEventArgs e)
        {
            FeedbinEntry item = e.ClickedItem as FeedbinEntry;

            if (item != null)
            {
                Frame.Navigate(typeof(ArticlePage), item);
            }
        }

    }
}
