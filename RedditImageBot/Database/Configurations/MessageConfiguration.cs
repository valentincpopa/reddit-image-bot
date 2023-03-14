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
            builder.Property(x => x.Status).HasColumnType("smallint").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("TimestampTz").IsRequired();
            builder.Property(x => x.ModifiedAt).HasColumnType("TimestampTz").IsRequired();

            builder.Ignore(x => x.MessageStateManager);
        }
    }
}
