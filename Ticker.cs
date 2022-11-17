using LightServer.Types;
using NPSMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using WinRT;

namespace LightServer
{

    internal class Ticker
    {
        private static Ticker singletonInstance = new Ticker();
        private List<Action<PlayerState>> delegateList = new List<Action<PlayerState>>();

        private static Lazy<NowPlayingSessionManager> sessionManager = new Lazy<NowPlayingSessionManager>(() => new NPSMLib.NowPlayingSessionManager());
        private static bool sessionManagerRegistered = false;
        private static MediaPlaybackDataSource dataSource = null;

        public static PlayerState playerState = new PlayerState();

        private Ticker()
        {
            var timer = new Timer(5);
            timer.Elapsed += RunEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private static void UpdateSessions()
        {
            if (dataSource == null || dataSource.GetMediaTimelineProperties().EndTime.Ticks == 0)
            {
                var sessions = Ticker.sessionManager.Value.GetSessions();
                var validSessions = sessions.Where(elm => elm.ActivateMediaPlaybackDataSource().GetMediaTimelineProperties().PositionSetFileTime > DateTime.UnixEpoch);
                var nextSession = validSessions.FirstOrDefault();
                if (nextSession != null)
                {
                    Ticker.sessionManager.Value.SetCurrentSession(nextSession.GetSessionInfo());
                    dataSource = nextSession.ActivateMediaPlaybackDataSource();
                }
            }
            Ticker.UpdatePlayerState();
        }

        private static void OnSessionListChanged(object sender, NowPlayingSessionManagerEventArgs e)
        {
            if (e.NotificationType == NowPlayingSessionManagerNotificationType.CurrentSessionChanged)
            {
                dataSource = Ticker.sessionManager.Value.CurrentSession.ActivateMediaPlaybackDataSource();
            }
            UpdateSessions();
        }

        public static Ticker GetInstance
        {
            get
            {
                return singletonInstance;
            }
        }

        private static void UpdatePlayerState()
        {
            if (!sessionManagerRegistered)
            {
                sessionManager.Value.SessionListChanged += OnSessionListChanged;
                sessionManagerRegistered = true;
                UpdateSessions();
            }
            if (dataSource == null) return;
            var ds = dataSource;
            var playbackInfos = ds.GetMediaPlaybackInfo();
            var objectInfos = ds.GetMediaObjectInfo();
            var timelineProps = ds.GetMediaTimelineProperties();
            var position = playbackInfos.PlaybackState == NPSMLib.MediaPlaybackState.Playing ? DateTime.Now - timelineProps.PositionSetFileTime + timelineProps.Position : timelineProps.Position;
            var ticker = Ticker.GetInstance;
            playerState.PlayState = playbackInfos.PlaybackState;
            playerState.Position = position;
            playerState.Duration = timelineProps.MaxSeekTime;
            playerState.Title = objectInfos.Title;
            playerState.Artist = objectInfos.Artist ?? objectInfos.AlbumArtist;
            playerState.AlbumImage = ds.GetThumbnailStream();
            ticker.delegateList.ForEach((callback) => App.Current.As<App>().dispatcherQueue.TryEnqueue(() => callback(playerState)));
        }

        private static void RunEvent(object source, ElapsedEventArgs e)
        {
            Ticker.UpdatePlayerState();
        }

        public static void AddCallback(Action<PlayerState> callback)
        {
            var ticker = Ticker.GetInstance;
            ticker.delegateList.Add(callback);
        }

        public static bool RemoveCallback(Action<PlayerState> callback)
        {
            var ticker = Ticker.GetInstance;
            return ticker.delegateList.Remove(callback);
        }
    }
}
