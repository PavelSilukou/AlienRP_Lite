using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace AlienRP_Lite
{
    public sealed partial class ChannelItem : UserControl
    {
        PlayerPage playerPage;
        Channel channel = null;

        public ChannelItem()
        {
            this.InitializeComponent();
        }

        public ChannelItem(PlayerPage playerPage, Channel channel)
        {
            this.InitializeComponent();

            this.playerPage = playerPage;
            this.channel = channel;
            try
            {
                Image.ImageSource = new BitmapImage(new Uri(channel.imageUrl));
            }
            catch (ArgumentNullException)
            {
                ShowDefaultImage();
            }
            catch (UriFormatException)
            {
                ShowDefaultImage();
            }
            channelName.Text = channel.name;
        }

        private void ShowDefaultImage()
        {
            imageBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            imageDefaultBorder.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void outerBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            imageBorder.Opacity = 1.0;
        }

        private void outerBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            playerPage.ChannelChecked(this.channel);
        }

        private void outerBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            imageBorder.Opacity = 0.7;
        }
    }
}
