using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RedditImageBot.Database.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("posts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ExternalId).HasColumnType("varchar(16)").IsRequired();
            builder.Property(x => x.Status).HasField("_status").HasColumnType("smallint").UsePropertyAccessMode(PropertyAccessMode.Property).IsRequired();
            builder.Property(x => x.GeneratedImageUrl).HasColumnType("varchar(1024)");
            builder.Property(x => x.CreatedAt).HasColumnType("TimestampTz").IsRequired();
            builder.Property(x => x.ModifiedAt).HasColumnType("TimestampTz").IsRequired();

            builder.Ignore(x => x.PostStateManager);

            builder.HasMany(x => x.Messages).WithOne(x => x.Post).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        }
    }
}
