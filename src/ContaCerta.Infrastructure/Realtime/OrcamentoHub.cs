using Microsoft.AspNetCore.SignalR;

namespace ContaCerta.Infrastructure.Realtime;

/// <summary>
/// Hub e so o canal de push: o dashboard conecta e escuta o evento "AlertaOrcamento",
/// nenhum metodo de cliente para servidor e necessario aqui hoje.
/// </summary>
public class OrcamentoHub : Hub
{
}
