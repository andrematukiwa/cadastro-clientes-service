using CadastroClientes.Domain.Clientes;
using CadastroClientes.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroClientes.Infrastructure.Clientes;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(Cliente.NomeTamanhoMaximo).IsRequired();
        builder.Property(c => c.Cpf).HasMaxLength(Cpf.Tamanho).IsFixedLength().IsRequired();
        builder.Property(c => c.Email).HasMaxLength(Cliente.EmailTamanhoMaximo).IsRequired();
        builder.Property(c => c.DataNascimento).IsRequired();
        builder.Property(c => c.RendaMensal)
            .HasPrecision(Cliente.RendaPrecisao, Cliente.RendaCasasDecimais)
            .IsRequired();
        builder.Property(c => c.CriadoEm).IsRequired();

        // Garante a unicidade mesmo com duas requisições simultâneas para o mesmo CPF
        builder.HasIndex(c => c.Cpf).IsUnique();
    }
}
