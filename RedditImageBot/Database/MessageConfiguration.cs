using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Database
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("tblMessages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Fullname).HasColumnType("varchar(16)").IsRequired();
            builder.Property(x => x.IsProcessed).HasColumnType("boolean").HasDefaultValue(0).IsRequired();
        }
    }
}
