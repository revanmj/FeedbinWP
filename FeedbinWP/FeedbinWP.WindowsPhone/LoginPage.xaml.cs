using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using FeedbinWP.Services;
using FeedbinWP.Data;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace FeedbinWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void LogIn_Click(object sender, RoutedEventArgs e)
        {
            String username = email.Text;
            String password = passwd.Password;

            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            progressbar.Text = "Logging in ...";

            await progressbar.ShowAsync();

            bool result = await FeedbinSyncSqlite.Login(username, password);

            await progressbar.HideAsync();

            if (result)
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                vault.Add(new Windows.Security.Credentials.PasswordCredential("Feedbin", username, password));

                SettingsData settings = new SettingsData();
                settings.readSettings();
                settings.loggedIn = true;
                settings.saveSettings();

                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null && rootFrame.CanGoBack)
                    rootFrame.GoBack();
                else if (!rootFrame.CanGoBack)
                    Frame.Navigate(typeof(ArticleListPage));
            } else
            {
                MessageDialog msgbox = new MessageDialog("Login unsuccesful!");
                await msgbox.ShowAsync();
            }

        }
    }
}
