using RandomFy.ApiService.Services;
using SpotifyAPI.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<SpotifyService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Endpoint principal para buscar canciones e inicializar las estructuras de datos
app.MapPost("api/spotify/buscar", async (string termino, SpotifyService service) => {
    if (string.IsNullOrEmpty(termino)) return Results.BadRequest();

    var cliente = await service.ObtenerClienteSpotify();
    await service.BuscarYEncolar(termino, cliente);
    return Results.Ok();
});

// Endpoints para recuperar el estado actual de las estructuras lineales (Cola y Pila)
app.MapGet("api/spotify/cola", (SpotifyService service) =>
    Results.Ok(service.ColaDeReproduccion.ToList()));

app.MapGet("api/spotify/pila", (SpotifyService service) =>
    Results.Ok(service.HistorialDeReproduccion.ToList()));

// Ejecuta la transición de la canción en reproducción desde la Cola hacia el Historial
app.MapPost("api/spotify/reproducir", (SpotifyService service) => {
    var cancion = service.ReproducirSiguiente();
    return cancion != null ? Results.Ok(cancion) : Results.NotFound();
});

// Devuelve los artistas ordenados alfabéticamente recorriendo el Árbol Binario (In-Order)
app.MapGet("api/spotify/artistas", (SpotifyService service) =>
    Results.Ok(service.ObtenerArtistasOrdenados()));

// Endpoints para interactuar con la Tabla Hash de canciones favoritas
app.MapGet("api/spotify/favoritos", (SpotifyService service) =>
    Results.Ok(service.ObtenerFavoritos()));

app.MapPost("api/spotify/favoritos", (FullTrack track, SpotifyService service) => {
    service.AgregarAFavoritos(track);
    return Results.Ok();
});

app.MapPost("api/spotify/favoritos/{id}", (string id, SpotifyService service) => {
    var track = service.ColaDeReproduccion.FirstOrDefault(t => t.Id == id);
    if (track != null)
    {
        service.AgregarAFavoritos(track);
        return Results.Ok();
    }
    return Results.NotFound();
});


// Endpoints para visualizar y estructurar las conexiones del Grafo Musical
app.MapGet("api/spotify/grafo", (SpotifyService service) =>
    Results.Ok(service.GrafoArtistasSimilares));

app.MapPost("api/spotify/grafo/{id}/{nombre}", async (string id, string nombre, SpotifyService service) => {
    var cliente = await service.ObtenerClienteSpotify();
    await service.GenerarGrafoDesdeArtista(id, nombre, cliente);
    return Results.Ok();
});

app.Run();
