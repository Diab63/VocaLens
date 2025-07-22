using Microsoft.EntityFrameworkCore;
using VocaLens.Models;

namespace VocaLens.Data
{
    public class AudioDbContext : DbContext
    {
        public DbSet<AudioRecording> AudioRecordings { get; set; }
        public AudioDbContext(DbContextOptions<AudioDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
