using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
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
using Windows.ApplicationModel.Resources;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FeedbinWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ArticlePage : Page
    {
        FeedbinEntry entry;
        DataTransferManager _dataTransferManager;
        String style;
        StatusBarProgressIndicator progressbar;
        ResourceLoader loader;

        public ArticlePage()
        {
            this.InitializeComponent();

            progressbar = StatusBar.GetForCurrentView().ProgressIndicator;

            loader = new ResourceLoader();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            entry = e.Parameter as FeedbinEntry;

            if (entry.starred)
            {
                starButton.Icon = new SymbolIcon(Symbol.UnFavorite);
                starButton.Label = loader.GetString("removeStar");
            } else
            {
                starButton.Label = loader.GetString("addStar"); ;
                starButton.Icon = new SymbolIcon(Symbol.Favorite);
            }

            if (!entry.read)
                FeedbinSyncSqlite.markSingleAsRead(entry);

            style = "";
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/ReadingViewStyle.css"));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                style = await sRead.ReadToEndAsync();

            webview.NavigateToString(style + "<h1>" + entry.title + "</h1>" + entry.author + "</br>" + entry.feed_id + "</br></br>" + entry.content);

            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += OnDataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister the current page as a share source.
            _dataTransferManager.DataRequested -= OnDataRequested;
        }

        protected void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            e.Request.Data.Properties.Title = entry.title;
            e.Request.Data.SetWebLink(new Uri(entry.url));
        }

        private async void Web_Click(Object sender, RoutedEventArgs e)
        {
            var uri = new Uri(entry.url);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private async void Readability_Click(Object sender, RoutedEventArgs e)
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Parsing via Readability ...";
            await progressbar.ShowAsync();

            String newContent = await ReadabilityParser.parseViaReadability(entry.url);
            if (newContent != null)
                webview.NavigateToString(style + "<h1>" + entry.title + "</h1>" + entry.author + "</br>" + entry.feed_id + "</br></br>" + newContent);
            else
            {
                MessageDialog msg = new MessageDialog("Readability error.");
                await msg.ShowAsync();
            }

            await progressbar.HideAsync();
        }

        private void Share_Click(Object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void Star_Click(Object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;

            Utils.ShowToastNotification("Operation failed");

            //if (button.Label == "Star")
            //{
            //    await FeedbinSyncSqlite.addSingleStar(entry);

            //    button.Label = "Remove star";
            //    button.Icon = new SymbolIcon(Symbol.UnFavorite);
            //} else if (button.Label == "Remove star")
            //{
            //    await FeedbinSyncSqlite.removeSingleStar(entry);

            //    button.Label = "Star";
            //    button.Icon = new SymbolIcon(Symbol.Favorite);
            //}
        }

        private async void Read_Click(Object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;

            if (button.Label == "Mark as unread")
            {
                await FeedbinSyncSqlite.markSingleAsUnread(entry);

                button.Label = "Mark as read";
            } else if (button.Label == "Mark as read")
            {
                await FeedbinSyncSqlite.markSingleAsRead(entry);

                button.Label = "Mark as unread";
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (webview.CanGoBack)
            {
                webview.GoBack();
                e.Handled = true;
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null && rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();
                    e.Handled = true;
                }
            }
        }

        private async void webview_LoadCompleted(object sender, NavigationEventArgs e)
        {
            await progressbar.HideAsync();
        }

        private async void webview_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            progressbar.Text = "";
            await progressbar.ShowAsync();
        }
    }
}
