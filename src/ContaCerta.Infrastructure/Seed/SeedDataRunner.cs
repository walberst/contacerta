using ContaCerta.Domain.Entities;
using ContaCerta.Domain.Enums;
using ContaCerta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContaCerta.Infrastructure.Seed;

/// <summary>
/// Popula dados de demonstracao coerentes com o nicho (MEI misturando pessoal e
/// negocio). Roda apenas quando chamado explicitamente (dotnet run -- --seed), nunca
/// automaticamente na subida da API, para nao contaminar um banco de uso real.
/// </summary>
public class SeedDataRunner
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SeedDataRunner> _logger;

    public SeedDataRunner(ApplicationDbContext context, ILogger<SeedDataRunner> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);

        if (await _context.Contas.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Seed ignorado: ja existem dados no banco.");
            return;
        }

        var contaNegocio = Conta.Criar("Conta PJ Nubank", TipoConta.Negocio, 5000m);
        var contaPessoal = Conta.Criar("Conta pessoal Itau", TipoConta.Pessoal, 1200m);
        _context.Contas.AddRange(contaNegocio, contaPessoal);

        var fornecedores = Categoria.Criar("Fornecedores", TipoLancamento.Despesa, 3000m);
        var aluguel = Categoria.Criar("Aluguel do escritorio", TipoLancamento.Despesa, 1500m);
        var marketing = Categoria.Criar("Marketing", TipoLancamento.Despesa, 800m);
        var sistemas = Categoria.Criar("Assinaturas de sistema", TipoLancamento.Despesa, 300m);
        var alimentacao = Categoria.Criar("Alimentacao", TipoLancamento.Despesa, 900m);
        var transporte = Categoria.Criar("Transporte", TipoLancamento.Despesa, 400m);
        var vendas = Categoria.Criar("Vendas", TipoLancamento.Receita, 0m);
        var salario = Categoria.Criar("Retirada de pro labore", TipoLancamento.Receita, 0m);
        _context.Categorias.AddRange(fornecedores, aluguel, marketing, sistemas, alimentacao, transporte, vendas, salario);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioDoMes = new DateOnly(hoje.Year, hoje.Month, 1);
        var mesPassado = inicioDoMes.AddMonths(-1);

        var lancamentos = new List<Lancamento>
        {
            // Recorrentes: formam a base da projecao de fluxo de caixa dos proximos 30 dias.
            Lancamento.Criar(contaNegocio.Id, aluguel.Id, TipoLancamento.Despesa, 1500m, mesPassado.AddDays(4), "Aluguel do escritorio", Recorrencia.Mensal),
            Lancamento.Criar(contaNegocio.Id, sistemas.Id, TipoLancamento.Despesa, 249m, mesPassado.AddDays(9), "Assinatura ERP", Recorrencia.Mensal),
            Lancamento.Criar(contaNegocio.Id, vendas.Id, TipoLancamento.Receita, 6500m, mesPassado.AddDays(4), "Contrato de manutencao cliente fixo", Recorrencia.Mensal),
            Lancamento.Criar(contaPessoal.Id, salario.Id, TipoLancamento.Receita, 3000m, mesPassado.AddDays(4), "Pro labore mensal", Recorrencia.Mensal),

            // Despesas avulsas do negocio no mes corrente, propositalmente somando perto do
            // orcamento de fornecedores para a tela de orcamento mostrar a barra quase cheia.
            Lancamento.Criar(contaNegocio.Id, fornecedores.Id, TipoLancamento.Despesa, 1200m, inicioDoMes.AddDays(2), "Compra de insumos de producao"),
            Lancamento.Criar(contaNegocio.Id, fornecedores.Id, TipoLancamento.Despesa, 950m, inicioDoMes.AddDays(9), "Materia prima lote 2"),
            Lancamento.Criar(contaNegocio.Id, fornecedores.Id, TipoLancamento.Despesa, 400m, inicioDoMes.AddDays(15), "Embalagens"),
            Lancamento.Criar(contaNegocio.Id, marketing.Id, TipoLancamento.Despesa, 350m, inicioDoMes.AddDays(6), "Impulsionamento de post"),
            Lancamento.Criar(contaNegocio.Id, vendas.Id, TipoLancamento.Receita, 2800m, inicioDoMes.AddDays(11), "Vendas avulsas da semana"),

            // Pessoal: mostra claramente separado do negocio nos relatorios.
            Lancamento.Criar(contaPessoal.Id, alimentacao.Id, TipoLancamento.Despesa, 620m, inicioDoMes.AddDays(3), "Supermercado do mes"),
            Lancamento.Criar(contaPessoal.Id, alimentacao.Id, TipoLancamento.Despesa, 180m, inicioDoMes.AddDays(14), "Restaurante"),
            Lancamento.Criar(contaPessoal.Id, transporte.Id, TipoLancamento.Despesa, 210m, inicioDoMes.AddDays(7), "Combustivel"),

            // Historico do mes passado, para o dashboard ter comparacao.
            Lancamento.Criar(contaNegocio.Id, fornecedores.Id, TipoLancamento.Despesa, 2100m, mesPassado.AddDays(12), "Compra de insumos"),
            Lancamento.Criar(contaPessoal.Id, alimentacao.Id, TipoLancamento.Despesa, 700m, mesPassado.AddDays(10), "Supermercado")
        };

        _context.Lancamentos.AddRange(lancamentos);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seed concluido: {Contas} contas, {Categorias} categorias, {Lancamentos} lancamentos.",
            2, 8, lancamentos.Count);
    }
}
