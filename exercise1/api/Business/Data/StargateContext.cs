using Microsoft.EntityFrameworkCore;
using System.Data;

namespace StargateAPI.Business.Data
{
    public class StargateContext : DbContext
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<Person> People { get; set; }
        public DbSet<AstronautDetail> AstronautDetails { get; set; }
        public DbSet<AstronautDuty> AstronautDuties { get; set; }
        public DbSet<ProcessLog> ProcessLogs { get; set; }

        public StargateContext(DbContextOptions<StargateContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);

            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Person data
            modelBuilder.Entity<Person>()
                .HasData(
                    new Person { Id = 1, Name = "John Doe" },
                    new Person { Id = 2, Name = "Jane Doe" },
                    new Person { Id = 3, Name = "Samantha Carter" },
                    new Person { Id = 4, Name = "Daniel Jackson" }
                );

            // Seed AstronautDetail data
            modelBuilder.Entity<AstronautDetail>()
                .HasData(
                    new AstronautDetail
                    {
                        Id = 1,
                        PersonId = 1,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = new DateTime(2010, 1, 1),
                        CareerEndDate = null
                    },
                    new AstronautDetail
                    {
                        Id = 2,
                        PersonId = 3,
                        CurrentRank = "Major",
                        CurrentDutyTitle = "Science Officer",
                        CareerStartDate = new DateTime(2012, 5, 15),
                        CareerEndDate = null
                    }
                );

            // Seed AstronautDuty data
            modelBuilder.Entity<AstronautDuty>()
                .HasData(
                    new AstronautDuty
                    {
                        Id = 1,
                        PersonId = 1,
                        Rank = "1LT",
                        DutyTitle = "Commander",
                        DutyStartDate = new DateTime(2010, 1, 1),
                        DutyEndDate = new DateTime(2015, 12, 31)
                    },
                    new AstronautDuty
                    {
                        Id = 2,
                        PersonId = 1,
                        Rank = "Captain",
                        DutyTitle = "Mission Lead",
                        DutyStartDate = new DateTime(2016, 1, 1),
                        DutyEndDate = null
                    },
                    new AstronautDuty
                    {
                        Id = 3,
                        PersonId = 3,
                        Rank = "Major",
                        DutyTitle = "Science Officer",
                        DutyStartDate = new DateTime(2012, 5, 15),
                        DutyEndDate = null
                    },
                    new AstronautDuty
                    {
                        Id = 4,
                        PersonId = 2,
                        Rank = "Lieutenant",
                        DutyTitle = "Pilot",
                        DutyStartDate = new DateTime(2018, 3, 10),
                        DutyEndDate = null
                    }
                );
        }
    }
}
