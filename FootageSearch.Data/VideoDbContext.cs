using Microsoft.EntityFrameworkCore;
using FootageSearch.Data.Models;
using System;
using System.IO;

namespace FootageSearch.Data
{
    public class VideoDbContext : DbContext
    {
        public DbSet<VideoFile> VideoFiles { get; set; }
        public DbSet<JobStatusEntity> JobStatuses { get; set; }

        public string DbPath { get; }

        public VideoDbContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "FootageSearch", "footage.db");
            
            // Ensure directory exists
            var dir = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={DbPath}");
            }
        }
    }
}
