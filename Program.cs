using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


var notes = new List<Note>();

var app = builder.Build();
app.MapGet("/health", () => new { status = "ok", time = DateTime.Now });

app.MapGet("/version", (IConfiguration conf) => new {
    name = conf["App:Name"],
    version = conf["App:Version"]
});

app.MapGet("/api/notes", () => notes);
app.MapPost("/api/notes", (Note note) => {
    if (string.IsNullOrEmpty(note.Title)) return Results.BadRequest("Заголовок не может быть пустым");

    var newNote = note with { Id = notes.Count + 1, CreatedAt = DateTime.Now };
    notes.Add(newNote);
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
}); 
app.MapGet("/api/notes/{id}", (int id) =>
    notes.FirstOrDefault(n => n.Id == id) is Note note ? Results.Ok(note) : Results.NotFound());
app.MapDelete("/api/notes/{id}", (int id) => {
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note == null) return Results.NotFound();
    notes.Remove(note);
    return Results.NoContent();
});

app.MapGet("/db/ping", (IConfiguration conf) => {
    var connectionString = conf.GetConnectionString("Mssql");

    try
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        return Results.Ok(new { status = "ok", message = "Database connection successful" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record Note(int Id, string Title, string Text, DateTime CreatedAt);