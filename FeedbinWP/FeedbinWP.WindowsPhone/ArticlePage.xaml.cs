using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public ArticlePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            entry = e.Parameter as FeedbinEntry;
            titleBox.Text = entry.title;

            style = "";
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new System.Uri("ms-appx:///Assets/ReadingViewStyle.css"));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                style = await sRead.ReadToEndAsync();

            webview.NavigateToString(style + entry.content);

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
        {;
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
                webview.NavigateToString(style + newContent);
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
    }
}
