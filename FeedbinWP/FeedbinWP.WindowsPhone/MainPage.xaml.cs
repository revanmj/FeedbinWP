using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FeedbinWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FeedbinData data;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            data = new FeedbinData();
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
        }

        private void Settings_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void LogIn_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void AddSubs_Click(Object sender, RoutedEventArgs e)
        {

        }

        private async void Sync_Click(Object sender, RoutedEventArgs e)
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Syncing entries ...";
            await progressbar.ShowAsync();

            var vault = new Windows.Security.Credentials.PasswordVault();
            var credentialList = vault.FindAllByResource("Feedbin");
            PasswordCredential credential = credentialList[0];
            credential.RetrievePassword();

            ObservableCollection<FeedbinEntry> unreadEntries;
            ObservableCollection<FeedbinEntry> starredEntries;
            ObservableCollection<FeedbinEntry> recentEntries;
            //ObservableCollection<FeedbinEntry> allEntries;

            unreadEntries = await FeedbinSync.getUnreadItems(credential.UserName, credential.Password);
            unreadEntries =  new ObservableCollection<FeedbinEntry>(unreadEntries.OrderByDescending(f => f.published));
            starredEntries = await FeedbinSync.getStarredItems(credential.UserName, credential.Password);
            starredEntries = new ObservableCollection<FeedbinEntry>(starredEntries.OrderByDescending(f => f.published));
            recentEntries = await FeedbinSync.getRecentrlyRead(credential.UserName, credential.Password);
            recentEntries = new ObservableCollection<FeedbinEntry>(recentEntries.OrderByDescending(f => f.published));
            data.unreadEntries = unreadEntries;
            data.starredEntries = starredEntries;
            data.recentEntries = recentEntries;
            this.DataContext = null;
            this.DataContext = data;

            await progressbar.HideAsync();
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
