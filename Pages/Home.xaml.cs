using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LightServer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        DispatcherTimer timer = new DispatcherTimer();
        BitmapImage albumArt = new BitmapImage();
        Stream albumArtStreamRef = null;

        public Home()
        {
            this.InitializeComponent();
            albumArtImage.Source = albumArt;
        }

        private void TimerCallback(object sender, object e)
        {
            var state = Ticker.playerState;
            if (playingTime.Value != state.Position.TotalSeconds)
            {
                playingTime.Value = state.Position.TotalSeconds;
                playingTime.Maximum = state.Duration.TotalSeconds;
            }
            var shouldBePaused = state.PlayState == NPSMLib.MediaPlaybackState.Paused;
            if (playingTime.ShowPaused != shouldBePaused)
                playingTime.ShowPaused = shouldBePaused;
            var timeString = $"{state.Position.ToString("mm\\:ss")} / {state.Duration.ToString("mm\\:ss")}";
            if (playingTimeText.Text != timeString)
            {
                playingTimeText.Text = timeString;
            }
            var titleString = $"{state.Artist}  •  {state.Title}";
            if (state.Artist == "" && state.Title == "") titleString = "Unknown song";
            if (titleText.Text != titleString)
            {
                titleText.Text = titleString;
            }
            if (albumArtStreamRef != state.AlbumImage)
            {
                albumArtStreamRef = state.AlbumImage;
                if (albumArtStreamRef != null)
                {
                    _ = albumArt.SetSourceAsync(albumArtStreamRef.AsRandomAccessStream());
                }
                else
                {
                    albumArt.UriSource = null;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            TimerCallback(null, null);
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += TimerCallback;
            timer.Start();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timer.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}
