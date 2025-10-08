using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty; // INFO, ERROR, SUCCESS, WARNING
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? UserName { get; set; }
        public string? RequestData { get; set; }
    }

    public class ProcessLogConfiguration : IEntityTypeConfiguration<ProcessLog>
    {
        public void Configure(EntityTypeBuilder<ProcessLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Level).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Exception).HasMaxLength(4000);
            builder.Property(x => x.Controller).HasMaxLength(100);
            builder.Property(x => x.Action).HasMaxLength(100);
            builder.Property(x => x.UserName).HasMaxLength(100);
            builder.Property(x => x.RequestData).HasMaxLength(2000);
            builder.Property(x => x.Timestamp).IsRequired();
            
            // Index for performance
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.Level);
        }
    }
}