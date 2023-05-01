using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RedditImageBot.Database.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ExternalId).HasColumnType("varchar(16)").IsRequired();
            builder.Property(x => x.ExternalParentId).HasColumnType("varchar(16)").IsRequired();
            builder.Property(x => x.Status).HasField("_status").HasColumnType("smallint").UsePropertyAccessMode(PropertyAccessMode.Property).IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("TimestampTz").IsRequired();
            builder.Property(x => x.ModifiedAt).HasColumnType("TimestampTz").IsRequired();
            builder.Property(x => x.ProcessingCount).HasColumnType("smallint");
            builder.Property(b => b.Version).IsRowVersion();

            builder.Ignore(x => x.MessageStateManager);
        }
    }
}
