using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Database
{
    public class ProcessedPostConfiguration : IEntityTypeConfiguration<ProcessedPost>
    {
        public void Configure(EntityTypeBuilder<ProcessedPost> builder)
        {
            builder.ToTable("tblProcessedPosts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Fullname).HasColumnType("varchar(16)").IsRequired();
            builder.Property(x => x.ImageUrl).HasColumnType("varchar(1024)").IsRequired();

            builder.HasMany(x => x.Messages).WithOne(x => x.Post).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        }
    }
}
