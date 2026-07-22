using System.Net;
using System.Net.Http.Json;

namespace EduApoyos.IntegrationTests.Solicitudes;

public class SolicitudesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SolicitudesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Listar_SinToken_Retorna401()
    {
        var response = await _client.GetAsync("/api/solicitudes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Crear_SinToken_Retorna401()
    {
        var payload = new
        {
            estudianteId = Guid.NewGuid(),
            tipoApoyo = "Beca",
            montoSolicitado = 1000000,
            descripcion = "Solicitud de prueba"
        };

        var response = await _client.PostAsJsonAsync("/api/solicitudes", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerDetalle_SinToken_Retorna401()
    {
        var response = await _client.GetAsync($"/api/solicitudes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_SinToken_Retorna401()
    {
        var payload = new { nuevoEstado = "EnRevision", observacion = "test" };

        var response = await _client.PatchAsJsonAsync($"/api/solicitudes/{Guid.NewGuid()}/estado", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}