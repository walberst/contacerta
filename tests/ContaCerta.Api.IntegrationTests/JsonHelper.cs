using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContaCerta.Api.IntegrationTests;

/// <summary>
/// A Api serializa enums como string (configurado em Program.cs). O HttpClient de
/// teste, por padrao, nao usa essas opcoes, entao sem isso a leitura das respostas
/// falharia ao tentar converter a string de volta para o enum.
/// </summary>
public static class JsonHelper
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
