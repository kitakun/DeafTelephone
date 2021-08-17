namespace DeafTelephone.Web.Services.Persistence.EntityConfigurations
{
    using DeafTelephone.Web.Core.Domain;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class SettingRecordEntityConfiguration : IEntityTypeConfiguration<SettingRecord>
    {
        public void Configure(EntityTypeBuilder<SettingRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).IsRequired();

            builder.Property(x => x.Value).HasMaxLength(255).IsRequired();
        }
    }
}
