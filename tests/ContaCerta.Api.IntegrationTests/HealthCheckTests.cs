using System.Net;
using FluentAssertions;
using Xunit;

namespace ContaCerta.Api.IntegrationTests;

[Collection(SqlServerCollection.Nome)]
public class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(SqlServerFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Health_DeveResponderComSucesso()
    {
        var resposta = await _client.GetAsync("/health");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
