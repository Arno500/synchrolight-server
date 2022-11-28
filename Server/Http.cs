using DBModels;
using LightServer.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace LightServer.Server
{
    public class BeatSheetListShape
    {
        public string Title { get; set; }
        public string Artist { get; set; }
    }
    public class Http
    {
        public void Configure(WebApplication app)
        {
            app.MapGet("/", HealthCheck);
            app.MapGet("/health", HealthCheck);
            app.MapGet("/beatsheet/{artist}/{title}", GetBeatsheet);
            app.MapDelete("/beatsheet/{artist}/{title}", DeleteBeatsheet);
            app.MapPost("/beatsheet", StoreBeatsheet);
            app.MapGet("/beatsheet", ListBeatsheets);
        }

        private IResult HealthCheck(HttpContext context)
        {
            return Results.Ok("Server is working correctly!");
        }

        private async Task<BitSheet> GetBeatsheet(string artist, string title, HttpContext context)
        {
            Bitsheet beatsheet;
            using (var beatsheetContext = new BitsheetContext())
            {
                beatsheet = await beatsheetContext.Bitsheets.SingleAsync(b => b.Title == title && b.Artist == artist);
            }
            return JsonSerializer.Deserialize<BitSheet>(beatsheet.BitsheetData);
        }

        private async Task<IResult> StoreBeatsheet([FromBody] BitSheetDTO beatsheet, HttpContext context)
        {
            HttpRequestRewindExtensions.EnableBuffering(context.Request, 5 * 1000 * 1000);
            using (var beatsheetContext = new BitsheetContext())
            {
                var alreadyExisting = await beatsheetContext.Bitsheets.SingleOrDefaultAsync(b => b.Title == beatsheet.Metadata.Title && b.Artist == beatsheet.Metadata.Artist);
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                if (alreadyExisting != null)
                {
                    alreadyExisting.Artist = beatsheet.Metadata.Artist;
                    alreadyExisting.Title = beatsheet.Metadata.Title;
                    alreadyExisting.BitsheetData = JsonSerializer.Serialize(beatsheet, serializeOptions);
                    await beatsheetContext.SaveChangesAsync();
                }
                else
                {
                    var toStore = new Bitsheet();
                    toStore.Artist = beatsheet.Metadata.Artist;
                    toStore.Title = beatsheet.Metadata.Title;
                    toStore.BitsheetData = JsonSerializer.Serialize(beatsheet, serializeOptions);
                    await beatsheetContext.Bitsheets.AddAsync(toStore);
                }
                await beatsheetContext.SaveChangesAsync();
            }
            var urlEncoder = UrlEncoder.Create();
            return Results.Created($"/beatsheets/{urlEncoder.Encode(beatsheet.Metadata.Artist)}/{urlEncoder.Encode(beatsheet.Metadata.Title)}", beatsheet);
        }

        private IEnumerable<BeatSheetListShape> ListBeatsheets(HttpContext context)
        {
            List<BeatSheetListShape> beatsheets;
            using (var beatsheetContext = new BitsheetContext())
            {
                beatsheets = beatsheetContext.Bitsheets.Select(b => new BeatSheetListShape() { Title = b.Title, Artist = b.Artist }).ToList();
            }
            return beatsheets;
        }

        private async Task<IResult> DeleteBeatsheet(string artist, string title, HttpContext context)
        {
            using (var beatsheetContext = new BitsheetContext())
            {
                var alreadyExisting = await beatsheetContext.Bitsheets.SingleOrDefaultAsync(b => b.Title == title && b.Artist == artist);
                if (alreadyExisting == null)
                {
                    return Results.NotFound();
                }
                beatsheetContext.Bitsheets.Remove(alreadyExisting);
                await beatsheetContext.SaveChangesAsync();
            }
            return Results.Ok();
        }
    }
}
