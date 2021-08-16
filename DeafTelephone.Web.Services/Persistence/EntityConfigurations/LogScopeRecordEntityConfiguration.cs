﻿namespace DeafTelephone.Web.Services.Persistence.EntityConfigurations
{
    using System;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    using DeafTelephone.Web.Core.Domain;

    public class LogScopeRecordEntityConfiguration : IEntityTypeConfiguration<LogScopeRecord>
    {
        public void Configure(EntityTypeBuilder<LogScopeRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.RootScopeId);

            builder.Property(x => x.OwnerScopeId);
        }
    }
}
