using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DBModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

        private async Task<Beatsheet> GetBeatsheet(string artist, string title, HttpContext context)
        {
            Beatsheet beatsheet;
            using (var beatsheetContext = new BeatsheetContext())
            {
                beatsheet = await beatsheetContext.Beatsheets.SingleAsync(b => b.Title == title && b.Artist == artist);
            }
            return beatsheet;
        }

        private async Task<IResult> StoreBeatsheet([FromBody] Beatsheet beatsheet, HttpContext context)
        {
            using (var beatsheetContext = new BeatsheetContext())
            {
                var alreadyExisting = await beatsheetContext.Beatsheets.SingleOrDefaultAsync(b => b.Title == beatsheet.Title && b.Artist == beatsheet.Artist);
                if (alreadyExisting != null)
                {
                    var entry = beatsheetContext.Entry(alreadyExisting);
                    // TODO: Needs testing
                    entry.CurrentValues.SetValues(beatsheet);
                } else
                {
                    await beatsheetContext.Beatsheets.AddAsync(beatsheet);
                }
                await beatsheetContext.SaveChangesAsync();
            }
            return Results.Created($"/beatsheets/{beatsheet.Artist}/{beatsheet.Title}", beatsheet);
        }

        private IEnumerable<BeatSheetListShape> ListBeatsheets(HttpContext context)
        {
            List<BeatSheetListShape> beatsheets;
            using (var beatsheetContext = new BeatsheetContext())
            {
                beatsheets = beatsheetContext.Beatsheets.Select(b => new BeatSheetListShape() { Title = b.Title, Artist = b.Artist }).ToList();
            }
            return beatsheets;
        }

        private async Task<IResult> DeleteBeatsheet(string artist, string title, HttpContext context)
        {
            using (var beatsheetContext = new BeatsheetContext())
            {
                var alreadyExisting = await beatsheetContext.Beatsheets.SingleOrDefaultAsync(b => b.Title == title && b.Artist == artist);
                if (alreadyExisting == null)
                {
                    return Results.NotFound();
                }
                beatsheetContext.Beatsheets.Remove(alreadyExisting);
                await beatsheetContext.SaveChangesAsync();
            }
            return Results.Ok();
        }
    }
}
