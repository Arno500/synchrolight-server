using DBModels;
using LightServer.Server;
using LightServer.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LightServer.Managers
{
    internal class BitSheetManager
    {
        private BitSheet currentBitSheet = null;
        public static bool foundBitSheet = false;

        private static volatile bool alreadyRunning = false;

        private List<LightEvent> events = new();
        private int currentIndex = 0;
        private LightEvent? currentEvent = null;

        public BitSheetManager()
        {
            Ticker.AddCallback(ProcessBeatsheetAsync);
        }

        public async void ProcessBeatsheetAsync(PlayerState state, bool newTrack)
        {
            if (state.Title == null || state.Artist == null) return;
            if (alreadyRunning) return;
            alreadyRunning = true;
            if (newTrack || currentBitSheet == null)
            {
                Console.WriteLine("New song, loading BitSheet");
                await LoadBitSheet(state);
            }
            alreadyRunning = false;
            if (events == null) return;
            LightEvent ?checkEvent;
            if (currentIndex >= events.Count)
            {
                checkEvent = null;
            } else
            {
                checkEvent = events[currentIndex];
            }
            var stopPoint = DateTime.MinValue.Add(state.Position.Add(TimeSpan.FromMilliseconds(Ticker.GRANULARITY)));
            // In case of incoherency due to seeking, try to find the good event
            if (
                (currentIndex == events.Count && events.Last().Start > stopPoint) ||
                (currentIndex + 1 < events.Count && events?[currentIndex+1].Start < stopPoint) ||
                (currentIndex - 1 > 0 && events?[currentIndex - 1].Start > stopPoint) ||
                (currentEvent.HasValue && currentEvent.Value.Start > stopPoint)
                )
            {
                currentIndex = events.FindIndex(e => e.Start > stopPoint);
                // We are currently inside of another event
                if (currentIndex - 1 > 0 && events[currentIndex - 1].End > stopPoint)
                {
                    currentIndex--;
                    checkEvent = events[currentIndex];
                    currentEvent = checkEvent;
                } else if (currentIndex == 0)
                {
                    checkEvent = events[0];
                    currentEvent = null;
                    _ = WebSocket.SendEvent();
                }
                else if (currentIndex < 0)
                {
                    currentIndex = events.Count;
                    _ = WebSocket.SendEvent();
                    currentEvent = null;
                } else
                {
                    _ = WebSocket.SendEvent();
                }
            }
            var foundEvent = false;
            if (checkEvent?.Start < stopPoint && checkEvent?.End > stopPoint)
            {
                foundEvent = true;
                currentEvent = checkEvent;
                currentIndex++;
                _ = WebSocket.SendEvent(currentEvent.Value);
            }
            // If there is an event running and no new event was triggered, reset and remove the old one
            if (!foundEvent && currentEvent.HasValue)
            {
                var absolutePosition = DateTime.MinValue.Add(state.Position);
                if (absolutePosition >= currentEvent.Value.End)
                {
                    _ = WebSocket.SendEvent();
                    currentEvent = null;
                }
            }
        }

        private async Task LoadBitSheet(PlayerState state)
        {
            BitSheetDTO bitsheet;
            events = null;
            currentEvent = null;
            using (var bitsheetContext = new BitsheetContext())
            {
                var bs = await bitsheetContext.Bitsheets.SingleOrDefaultAsync(b => b.Title == state.Title && b.Artist == state.Artist);
                if (bs != null)
                {
                    bitsheet = JsonSerializer.Deserialize<BitSheetDTO>(bs.BitsheetData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    foundBitSheet = true;
                }
                else
                {
                    Console.WriteLine($"Could not find BitSheet for {state.Artist}/{state.Title}");
                    currentBitSheet = null;
                    foundBitSheet = false;
                    return;
                }
            }
            currentBitSheet = new BitSheet(bitsheet);
            events = currentBitSheet.Events;
        }
    }
}
