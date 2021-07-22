namespace DeafTelephone.Web.Services.Persistence.EntityConfigurations
{
    using System;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using DeafTelephone.Web.Core.Domain;

    public class LogRecordEntityConfiguration : IEntityTypeConfiguration<LogRecord>
    {
        public void Configure(EntityTypeBuilder<LogRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.LogLevel).IsRequired();
            builder.Property(x => x.Message).HasMaxLength(255).IsRequired();

            builder.Property(x => x.ErrorTitle).HasMaxLength(255);
            builder.Property(x => x.StackTrace).HasMaxLength(1024);

            builder.Property(x => x.RootScopeId);

            builder.HasOne(x => x.OwnerScope)
                .WithMany(x => x.InnerLogsCollection)
                .HasForeignKey(x => x.OwnerScopeId);
        }
    }
}
