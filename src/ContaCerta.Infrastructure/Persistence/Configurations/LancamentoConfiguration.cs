using ContaCerta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContaCerta.Infrastructure.Persistence.Configurations;

public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("Lancamentos");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.Valor).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Data).HasColumnType("date").IsRequired();
        builder.Property(l => l.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Recorrencia).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.DataCriacao).IsRequired();

        // Sem navegacao inversa em Conta/Categoria de proposito: nenhum caso de uso
        // precisa navegar "de conta para lancamentos" em memoria, so via repositorio
        // com filtro/paginacao, o que evita carregar colecoes gigantes sem querer.
        builder.HasOne(l => l.Conta)
            .WithMany()
            .HasForeignKey(l => l.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Categoria)
            .WithMany()
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.CategoriaId, l.Data });
        builder.HasIndex(l => new { l.ContaId, l.Data });
    }
}
