using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI.Popups;
using System.Text.RegularExpressions;
using Windows.Storage;

using Windows.UI.ViewManagement;

namespace AlienRP_Lite
{
    public sealed partial class MainPage : Page
    {

        private string email;
        private string password;

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            SolidColorBrush br = Application.Current.Resources["MainColor2"] as SolidColorBrush;
            titleBar.BackgroundColor = br.Color;

            br = Application.Current.Resources["AccentColor1"] as SolidColorBrush;
            titleBar.ForegroundColor = br.Color;

            br = Application.Current.Resources["MainColor2"] as SolidColorBrush;
            titleBar.ButtonBackgroundColor = br.Color;

            br = Application.Current.Resources["DarkFontColor1"] as SolidColorBrush;
            titleBar.ButtonForegroundColor = br.Color;

            br = Application.Current.Resources["MainColor2"] as SolidColorBrush;
            titleBar.ButtonHoverBackgroundColor = br.Color;

            br = Application.Current.Resources["AccentColor1"] as SolidColorBrush;
            titleBar.ButtonHoverForegroundColor = br.Color;

            br = Application.Current.Resources["MainColor2"] as SolidColorBrush;
            titleBar.ButtonPressedBackgroundColor = br.Color;

            br = Application.Current.Resources["AccentColor1"] as SolidColorBrush;
            titleBar.ButtonPressedForegroundColor = br.Color;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("rememberme"))
            {
                rememberMeCheckBox.IsChecked = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["rememberme"]);
            }

            if (rememberMeCheckBox.IsChecked == true)
            {
                try
                {
                    Windows.Security.Credentials.PasswordCredential credential = null;

                    var vault = new Windows.Security.Credentials.PasswordVault();
                    var credentialList = vault.FindAllByResource("AlienRPLite");

                    if (credentialList.Count > 0)
                    {
                        credential = credentialList[credentialList.Count-1];
                        credential.RetrievePassword();
                        email = credential.UserName;
                        password = credential.Password;
                        emailTextBox.Text = credential.UserName;
                        passwordTextBox.Password = credential.Password;
                    }
                }
                catch (Exception) { }
            }
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            emailTextBox.Tag = "";
            errorLoginLabel.Visibility = Visibility.Collapsed;

            passwordTextBox.Tag = "";
            errorPasswordLabel.Visibility = Visibility.Collapsed;

            generalErrorLabel.Visibility = Visibility.Collapsed;
            emailTextBox.Tag = "";
            passwordTextBox.Tag = "";

            if (emailTextBox.Text.Equals(""))
            {
                emailTextBox.Tag = "Error";
                errorLoginLabel.Visibility = Visibility.Visible;
                errorLoginLabel.Text = "The Email field cannot be empty";
                return;
            }
            else
            {
                bool isEmail = Regex.IsMatch(emailTextBox.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (!isEmail)
                {
                    emailTextBox.Tag = "Error";
                    errorLoginLabel.Visibility = Visibility.Visible;
                    errorLoginLabel.Text = "Invalid email address";
                    return;
                }
            }

            if (passwordTextBox.Password.Equals(""))
            {
                passwordTextBox.Tag = "Error";
                errorPasswordLabel.Visibility = Visibility.Visible;
                errorPasswordLabel.Text = "The Password field cannot be empty";
                return;
            }

            emailTextBox.IsEnabled = false;
            passwordTextBox.IsEnabled = false;
            loginButton.IsEnabled = false;

            Task<int> task = RadioAPI.Login(emailTextBox.Text, passwordTextBox.Password);
            int result = await task;

            switch (result)
            {
                case 0:
                    {
                        if (rememberMeCheckBox.IsChecked == true)
                        {
                            try
                            {
                                var vault = new Windows.Security.Credentials.PasswordVault();
                                var credentialList = vault.FindAllByResource("AlienRPLite");
                                if (credentialList.Count > 0)
                                {
                                    vault.Remove(new Windows.Security.Credentials.PasswordCredential("AlienRPLite", email, password));
                                }
                            }
                            catch (Exception) { }
                            try
                            {
                                var vault = new Windows.Security.Credentials.PasswordVault();
                                vault.Add(new Windows.Security.Credentials.PasswordCredential("AlienRPLite", emailTextBox.Text, passwordTextBox.Password));
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                var vault = new Windows.Security.Credentials.PasswordVault();
                                var credentialList = vault.FindAllByResource("AlienRPLite");
                                if (credentialList.Count > 0)
                                {
                                    vault.Remove(new Windows.Security.Credentials.PasswordCredential("AlienRPLite", email, password));
                                }
                            }
                            catch (Exception)
                            { }
                        }

                        ApplicationData.Current.LocalSettings.Values["rememberme"] = rememberMeCheckBox.IsChecked;

                        Frame.Navigate(typeof(PlayerPage));
                        break;
                    }
                case 1:
                    {
                        generalErrorLabel.Visibility = Visibility.Visible;
                        generalErrorLabel.Text = "Invalid login or password";
                        emailTextBox.Tag = "Error";
                        passwordTextBox.Tag = "Error";
                        break;
                    }
                case 2:
                    {
                        generalErrorLabel.Visibility = Visibility.Visible;
                        generalErrorLabel.Text = "Internet connection error. Check your connection and try again";
                        break;
                    }
                case 3:
                    {
                        generalErrorLabel.Visibility = Visibility.Visible;
                        generalErrorLabel.Text = "You are not a Premium Member of Digitally Imported";
                        break;
                    }
                case 4:
                    {
                        generalErrorLabel.Visibility = Visibility.Visible;
                        generalErrorLabel.Text = "You are not a Premium Member of Digitally Imported";
                        break;
                    }
                case 5:
                    {
                        generalErrorLabel.Visibility = Visibility.Visible;
                        generalErrorLabel.Text = "System error. Please email me at info@alienrp.com";
                        break;
                    }
            }

            emailTextBox.IsEnabled = true;
            passwordTextBox.IsEnabled = true;
            loginButton.IsEnabled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));
            ApplicationView.PreferredLaunchViewSize = new Size(500, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }
    }
}
