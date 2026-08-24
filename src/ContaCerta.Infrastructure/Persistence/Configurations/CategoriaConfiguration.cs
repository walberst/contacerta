using ContaCerta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContaCerta.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(80);
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.OrcamentoMensal).HasColumnType("decimal(18,2)");
        builder.Property(c => c.PercentuaisAlertaCsv).HasColumnName("PercentuaisAlerta").HasMaxLength(50).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();

        builder.Ignore(c => c.PercentuaisAlerta);
    }
}
