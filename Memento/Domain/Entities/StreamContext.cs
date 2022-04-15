using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Toolbelt.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StreamContext>
    {
        public StreamContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Memento/appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<StreamContext>();
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            builder.UseSqlServer(connectionString);
            return new StreamContext(builder.Options);
        }
    }

    public class StreamContext : DbContext
    {
        public StreamContext(DbContextOptions<StreamContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.BuildIndexesFromAnnotations();
            // use convention
            // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // already commented out v
            //modelBuilder.Conventions.Remove<OneToManyNoActionDeleteConvention>();

            modelBuilder.Entity<UserUser>()
                .HasOne(c => c.OwnerUser)
                .WithMany(c => c.ShareWithUser).IsRequired().IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<UserUser>()
               .HasOne(c => c.ReaderUser)
               .WithMany(c => c.SharedWithUser).IsRequired()
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SharingSuggestion>()
              .HasOne(c => c.SuggestedUser)
               .WithMany(c => c.SuggestedTo).IsRequired()
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SharingSuggestion>()
                .HasOne(c => c.OwnerUser)
              .WithMany(c => c.SharingSuggestions).IsRequired()
              .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<ShareRequest>()             
               .HasOne(c => c.RequestingUser)
               .WithMany(c => c.MyRequests).IsRequired()
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShareRequest>()
              .HasOne(c => c.TargetUser)
              .WithMany(c => c.OthersRequestingMe).IsRequired()
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Drop>()
                .HasMany(c => c.TagDrops)
                .WithOne(r => r.Drop).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContentDrop>()
                .HasOne(x => x.Drop)
                .WithOne(x => x.ContentDrop).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserProfile>()
                .HasMany(c => c.Comments)
                .WithOne(r => r.Owner).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserProfile>()
                .HasMany(c => c.Albums)
                .WithOne(r => r.Owner).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserDrop>()
                .HasOne(c => c.Drop)
                .WithMany(m => m.OtherUsersDrops)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserDrop>()
                .HasOne(c => c.User)
                .WithMany(m => m.OtherPeoplesDrops)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TagDrop>()
                .HasOne(c => c.UserTag)
                .WithMany(r => r.TagDrops)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<PremiumPlan>()
                .HasOne(c => c.Transaction)
                .WithMany(c => c.PremiumPlans).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PremiumPlan>()
                .HasMany(c => c.ShareRequests)
                .WithOne(c => c.PremiumPlan).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PremiumPlan>()
               .HasMany(c => c.ChildrenPremiumPlans)
               .WithOne(c => c.ParentPremiumPlan).IsRequired()
               .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<SharedPlan>()
                .HasOne(x => x.SharedPremiumPlan)
                .WithMany(x => x.PlansSharedFrom).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<SharedDropNotification>()
                .HasOne(c => c.Sharer)
                .WithMany(c => c.TargetNotifications).IsRequired()
              .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<SharedDropNotification>()
                .HasOne(c => c.Target)
                .WithMany(c => c.MyNotifications).IsRequired()
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TagViewer>()
                .HasOne(c => c.Viewer).WithMany().IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserPrompt>()
                .HasIndex(u => new { u.PromptId, u.UserId })
                .IsUnique();

            modelBuilder.Entity<UserPromptAsker>()
                .HasOne(a => a.UserPrompt)
                .WithMany(a => a.Askers).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            /*
            modelBuilder.Entity<UserPrompt>()
                .HasMany(a => a.Askers)
                .WithOne(a => a.UserPrompt).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);*/

            modelBuilder.Entity<UserRelationship>()
               .HasIndex(u => new { u.UserId, u.Relationship })
               .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasMany(x => x.CreatedTimeLines)
                .WithOne(x => x.Owner).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserProfile>()
                .HasMany(x => x.TimelineDrops)
                .WithOne(x => x.UserProfile).IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TimelineUser>()
                .HasIndex(u => new { u.UserId, u.TimelineId })
                .IsUnique();

            modelBuilder.Entity<AlbumDrop>()
                .HasKey(c => new { c.AlbumId, c.DropId });

            modelBuilder.Entity<SharingSuggestion>()
                .HasKey(c => new { c.OwnerUserId, c.SuggestedUserId });

            modelBuilder.Entity<TagViewer>()
                .HasKey(c => new { c.UserTagId, c.UserId });

            modelBuilder.Entity<TimelineDrop>()
                .HasKey(c => new { c.TimelineId, c.DropId });
        }

        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumDrop> AlbumDrops { get; set; }
        public DbSet<AlbumExport> AlbumExports { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }
        public DbSet<ContentDrop> ContentDrops { get; set; }
        // public DbSet<ContentDroplet> ContentDroplets { get; set; }
        public DbSet<Drop> Drops { get; set; }
        // public DbSet<Droplet> Droplets { get; set; }
        public DbSet<PremiumPlan> PremiumPlans { get; set; }
        public DbSet<UserTag> UserNetworks { get; set; }
        public DbSet<UserUser> UserUsers { get; set; }
        public DbSet<TagDrop> NetworkDrops { get; set; }
        public DbSet<TagViewer> NetworkViewers { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<UserEmail> UserEmails { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<UserDrop> UserDrops { get; set; }
        public DbSet<ImageDrop> ImageDrops { get; set; }
        public DbSet<MovieDrop> MovieDrops { get; set; }
        public DbSet<ShareRequest> ShareRequests { get; set; }
        public DbSet<SharedDropNotification> SharedDropNotifications { get; set; }
        public DbSet<SharingSuggestion> SharingSuggestions { get; set; }
        public DbSet<SharedPlan> SharedPlans { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TimeMethod> TimeMethods { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Usage> Usages { get; set; }
        public DbSet<UserPrompt> UserPrompts { get; set; }
        public DbSet<UserPromptAsker> UserPromptAskers { get; set; }
        public DbSet<UserRelationship> UserRelationships { get; set; }
        public DbSet<Prompt> Prompts { get; set; }
        public DbSet<PromptTimeline> PromptTimelines { get; set; }
        public DbSet<Timeline> Timelines { get; set; }
        public DbSet<TimelineUser> TimelineUsers { get; set; }
        public DbSet<TimelineDrop> TimelineDrops { get; set; }
    }
}
