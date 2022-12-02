using LightServer.Types;
using NPSMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using WinRT;

namespace LightServer
{
    internal class TickerLock
        {
            static object lockObject = new object();
            static volatile bool isBusyFlag = false;

            internal static bool CanAcquire()
            {
                if (!isBusyFlag)
                    lock (lockObject)
                        if (!isBusyFlag) //could have changed by the time we acquired lock
                            return (isBusyFlag = true);
                return false;
            }

            internal static void Release()
            {
                lock (lockObject)
                    isBusyFlag = false;
            }
        }

    internal class Ticker
    {
        public const int GRANULARITY = 1;

        private static Ticker singletonInstance = new();
        private List<Action<PlayerState, bool>> delegateList = new();

        private static Lazy<NowPlayingSessionManager> sessionManager = new(() => new NPSMLib.NowPlayingSessionManager());
        private static volatile bool sessionManagerRegistered = false;
        private static MediaPlaybackDataSource dataSource = null;

        public static PlayerState playerState = new();

        private Ticker()
        {
            var timer = new Timer(GRANULARITY);
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

        public static Ticker GetInstance()
        {
           return singletonInstance;
        }

        private static void UpdatePlayerState()
        {
            if (!sessionManagerRegistered)
            {
                sessionManagerRegistered = true;
                UpdateSessions();
                sessionManager.Value.SessionListChanged += OnSessionListChanged;
            }
            if (dataSource == null) return;
            var ds = dataSource;
            var playbackInfos = ds.GetMediaPlaybackInfo();
            var changed = false;
            var timelineProps = ds.GetMediaTimelineProperties();
            var position = playbackInfos.PlaybackState == NPSMLib.MediaPlaybackState.Playing ? DateTime.Now - timelineProps.PositionSetFileTime + timelineProps.Position : timelineProps.Position;
            playerState.Position = position;
            playerState.Duration = timelineProps.MaxSeekTime;
            playerState.StartTime = timelineProps.PositionSetFileTime.Add(-timelineProps.Position);
            if (playbackInfos.PlaybackState == MediaPlaybackState.Changing || playerState.Artist == null || playerState.Title == null)
            {
                var objectInfos = ds.GetMediaObjectInfo();
                if (playerState.Title != objectInfos.Title)
                {
                    changed = true;
                    playerState.Title = objectInfos.Title;
                }
                if (playerState.Artist != (objectInfos.Artist ?? objectInfos.AlbumArtist))
                {
                    changed = true;
                    playerState.Artist = objectInfos.Artist ?? objectInfos.AlbumArtist;
                }
                playerState.AlbumImage = ds.GetThumbnailStream();
            }
            playerState.PlayState = playbackInfos.PlaybackState;
            Ticker.GetInstance().delegateList.ForEach((callback) => new Task(() => callback(playerState, changed)).Start());
        }

        private static void RunEvent(object source, ElapsedEventArgs e)
        {
            if (TickerLock.CanAcquire())
            {
                try
                {
                    Ticker.UpdatePlayerState();
                }
                finally
                {
                    TickerLock.Release();
                }
            }
        }

        public static void AddCallback(Action<PlayerState, bool> callback)
        {
            var ticker = Ticker.GetInstance();
            ticker.delegateList.Add(callback);
        }

        public static bool RemoveCallback(Action<PlayerState, bool> callback)
        {
            var ticker = Ticker.GetInstance();
            return ticker.delegateList.Remove(callback);
        }
    }
}
