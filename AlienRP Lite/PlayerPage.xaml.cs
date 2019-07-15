using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using System.Diagnostics;
using Windows.Storage;

namespace AlienRP_Lite
{
    public sealed partial class PlayerPage : Page
    {
        private List<Channel> channels = new List<Channel>();
        private bool isPlay = false;
        private Channel currentChannel;
        private Track currentTrack = new Track();
        private MediaPlayer player = new MediaPlayer();
        DispatcherTimer trackTimeTimer;
        private double trackTimeTicks = 0;

        public PlayerPage()
        {
            this.InitializeComponent();

            trackTimeTimer = new DispatcherTimer();
            trackTimeTimer.Tick += TrackTimeTick;
            trackTimeTimer.Interval = new TimeSpan(0, 0, 1);

            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(700, 700));
            //ApplicationView.PreferredLaunchViewSize = new Size(700, 700);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("volume"))
            {
                soundBar.Value = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["volume"]);
            }
            else
            {
                soundBar.Value = 50;
                ApplicationData.Current.LocalSettings.Values["volume"] = soundBar.Value;
            }

            player.Volume = soundBar.Value / 100;

            LoadChannels();
        }

        private async void GetNewTrack()
        {
            Task<Track> task = RadioAPI.GetNowPlayingTrack();
            Track result = await task;

            if (result == null)
            {
                ShowError();
            }
            else
            {
                Track newTrack = (Track)result;
                if (newTrack.trackName.Equals(currentTrack.trackName)) return;

                currentTrack = (Track)result;
                trackName.Text = currentTrack.trackName;
                artistName.Text = currentTrack.artistName;
                trackName1.Text = currentTrack.trackName;
                artistName1.Text = currentTrack.artistName;
                trackName2.Text = currentTrack.trackName;
                artistName2.Text = currentTrack.artistName;

                trackTimeTicks = (RadioAPI.ConvertToUnixTimestamp(DateTime.UtcNow) - PlayerSettings.timeOffset) - currentTrack.startTime;

            }
        }

        private void TrackTimeTick(object sender, object e)
        {
            if (trackTimeTicks == currentTrack.duration)
            {
                GetNewTrack();
            }
            else if (trackTimeTicks == currentTrack.duration + 5)
            {
                GetNewTrack();
            }
            else if (trackTimeTicks == currentTrack.duration + 10)
            {
                GetNewTrack();
            }
            else if (trackTimeTicks == currentTrack.duration + 15)
            {
                GetNewTrack();
            }
            trackTimeTicks += 1;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(700, 700));
            //ApplicationView.PreferredLaunchViewSize = new Size(700, 700);
            ///ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        public async void LoadChannels()
        {
            HideChannelsError();
            HideChannelsGrid();
            ShowChannelsInfo();
            if (channels.Count > 0)
            {
                channels.Clear();
            }
            if (channelsPanel.Children.Count > 0)
            {
                channelsPanel.Children.Clear();
            }

            Task<List<Channel>> task = RadioAPI.GetAllChannels();
            List<Channel> result = await task;

            if (result == null)
            {
                HideChannelsInfo();
                ShowChannelsError();
            }
            else
            {
                channels = result;

                foreach (Channel channel in channels)
                {
                    channel.imageUrl = channel.imageUrl + "?width=" + 300;
                    AddChannels(channel);
                }
                HideChannelsInfo();
                ShowChannelsGrid();
            }
        }

        private void AddChannels(Channel channel)
        {
            ChannelItem item = new ChannelItem(this, channel);
            channelsPanel.Children.Add(item);
        }

        public void ChannelChecked(Channel channel)
        {
            PlayerSettings.currentChannelId = channel.id;
            PlayerSettings.currentChannelKey = channel.key;

            currentChannel = channel;
            OnPlay();
        }

        public void OnPlay()
        {
            Stop();
            Play();
            playButton.IsChecked = true;
            
            //playButton1.IsChecked = true;
            //playButton2.IsChecked = true;
        }

        private async void Play()
        {
            isPlay = true;
            HideError();

            Task<string> task1 = RadioAPI.Ping();
            string result1 = await task1;

            Task<int> task = RadioAPI.GetAudioLinks();
            int result = await task;

            if (result == 1)
            {
                ShowError();
            }
            else
            {
                try
                {
                    player.Source = MediaSource.CreateFromUri(new Uri(PlayerSettings.audioStream));
                    channelName.Text = currentChannel.name;
                    channelName1.Text = currentChannel.name;
                    channelName2.Text = currentChannel.name;

                    contentPanel.Visibility = Visibility.Visible;

                    GetNewTrack();
                    //playButton.IsChecked = true;
                    //playButton1.IsChecked = true;
                    //playButton2.IsChecked = true;
                    trackTimeTimer.Start();
                    player.Play();
                }
                catch (ArgumentNullException)
                {
                    ShowError();
                }
                catch (UriFormatException)
                {
                    ShowError();
                }
            }
        }

        private void Stop()
        {
            isPlay = false;
            player.Pause();
            player.Source = null;
            trackTimeTimer.Stop();

            //playButton.IsChecked = false;
            //playButton1.IsChecked = false;
            //playButton2.IsChecked = false;
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (playButton.IsChecked == true)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }

        //private void PlayButtonClick1(object sender, RoutedEventArgs e)
        //{
        //    if (playButton1.IsChecked == true)
        //    {
        //        Play();
        //    }
        //    else
        //    {
        //        Stop();
        //    }
        //}

        //private void PlayButtonClick2(object sender, RoutedEventArgs e)
        //{
        //    if (playButton2.IsChecked == true)
        //    {
        //        Play();
        //    }
        //    else
        //    {
        //        Stop();
        //    }
        //}

        private void soundBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player.Volume = soundBar.Value / 100;
            ApplicationData.Current.LocalSettings.Values["volume"] = soundBar.Value;
            //soundBar1.Value = soundBar.Value;
            //soundBar2.Value = soundBar.Value;
        }

        private void soundBar_ValueChanged1(object sender, RangeBaseValueChangedEventArgs e)
        {
            //player.Volume = soundBar1.Value / 100;
            //soundBar.Value = soundBar1.Value;
            //soundBar2.Value = soundBar1.Value;
        }

        private void soundBar_ValueChanged2(object sender, RangeBaseValueChangedEventArgs e)
        {
            //player.Volume = soundBar2.Value / 100;
            //soundBar.Value = soundBar2.Value;
            //soundBar1.Value = soundBar2.Value;
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(700, 700));
            //ApplicationView.PreferredLaunchViewSize = new Size(700, 700);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void channelsPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int minWidth = 200;
            int maxWidth = 300;
            double stackPanelWidth = channelsPanel.ActualWidth;

            double integer = Math.Truncate(stackPanelWidth / maxWidth) + 1;
            double fraction = (stackPanelWidth / minWidth) - integer;
            double k = fraction / integer;

            int childrenCount = channelsPanel.Children.Count;
            for (int i = 0; i < childrenCount; i++)
            {
                ChannelItem channel = (ChannelItem)channelsPanel.Children[i];
                channel.Width = (int)(stackPanelWidth / integer);
            }
            //int minWidth = 200;
            //int maxWidth = 300;
            //double stackPanelWidth = channelsPanel.ActualWidth;

            //double integer = Math.Truncate(stackPanelWidth / minWidth);
            //double fraction = (stackPanelWidth / minWidth) - integer;
            //double k = fraction / integer;

            //int childrenCount = channelsPanel.Children.Count;
            //for (int i = 0; i < childrenCount; i++)
            //{
            //    ChannelItem channel = (ChannelItem)channelsPanel.Children[i];
            //    channel.Width = minWidth + (int)(minWidth * k);
            //}


            //double stackPanelWidth = channelsPanel.Width;
            //int childrenCount = channelsPanel.Children.Count;
            //if (stackPanelWidth <= 500)
            //{
            //    for (int i = 0; i < childrenCount; i++)
            //    {
            //        ChannelItem channel = (ChannelItem)channelsPanel.Children[i];
            //        channel.Width = channelsPanel.Width;
            //    }
            //}
            //else if (stackPanelWidth > 700 && stackPanelWidth < 1000)
            //{
            //    for (int i = 0; i < childrenCount; i++)
            //    {
            //        ChannelItem channel = (ChannelItem)channelsPanel.Children[i];
            //        channel.Width = (int)((int)channelsPanel.Width / 2);
            //    }
            //}
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double pageWidth = this.ActualWidth;

            if (pageWidth <= 600)
            {
                contentPanelV1.Visibility = Visibility.Visible;
                contentPanelV2.Visibility = Visibility.Collapsed;
                contentPanelV3.Visibility = Visibility.Collapsed;
                contentPanel.Height = 81;
            }
            else if (pageWidth <= 1000 && pageWidth > 600)
            {
                contentPanelV1.Visibility = Visibility.Collapsed;
                contentPanelV2.Visibility = Visibility.Visible;
                contentPanelV3.Visibility = Visibility.Collapsed;
                contentPanel.Height = 61;
            }
            else if (pageWidth > 1000)
            {
                contentPanelV1.Visibility = Visibility.Collapsed;
                contentPanelV2.Visibility = Visibility.Collapsed;
                contentPanelV3.Visibility = Visibility.Visible;
                contentPanel.Height = 61;
            }
        }

        private void ShowError()
        {
            contentPanel.Visibility = Visibility.Visible;
            errorGrid.Visibility = Visibility.Visible;
            playButton.IsChecked = false;
            Stop();
        }

        private void HideError()
        {
            if (errorGrid.Visibility == Visibility.Visible)
            {
                errorGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void tryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            LoadChannels();
        }

        private void ShowChannelsError()
        {
            errorChannelsGrid.Visibility = Visibility.Visible;
        }

        private void HideChannelsError()
        {
            errorChannelsGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowChannelsInfo()
        {
            loadingChannelsGrid.Visibility = Visibility.Visible;
        }

        private void HideChannelsInfo()
        {
            loadingChannelsGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowChannelsGrid()
        {
            channelsGrid.Visibility = Visibility.Visible;
        }

        private void HideChannelsGrid()
        {
            channelsGrid.Visibility = Visibility.Collapsed;
        }
    }
}
