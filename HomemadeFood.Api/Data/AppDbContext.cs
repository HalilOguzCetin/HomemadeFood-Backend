using HomemadeFood.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<ProducerProfile>
            ProducerProfiles
        { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Food> Foods { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<RecommendationSearch>
            RecommendationSearches
        { get; set; }

        public DbSet<RecommendationCandidate>
            RecommendationCandidates
        { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------------------------
            // USER
            // -------------------------------------------------

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasOne(x => x.ProducerProfile)
                    .WithOne(x => x.User)
                    .HasForeignKey<ProducerProfile>(
                        x => x.UserId);
            });

            // -------------------------------------------------
            // PRODUCER PROFILE
            // -------------------------------------------------

            modelBuilder.Entity<ProducerProfile>(entity =>
            {
                entity.Property(x => x.BusinessName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.VerificationStatus)
                    .HasMaxLength(30)
                    .IsRequired();
                entity.Property(x => x.RejectionReason)
    .HasMaxLength(500);

                entity.Property(x => x.Rating)
                    .HasPrecision(3, 2);

                entity.Property(x => x.CapacityDate)
                    .HasColumnType("date");

                entity.Property(x => x.CapacityVersion)
                    .IsConcurrencyToken()
                    .HasDefaultValue(1);
            });

            // -------------------------------------------------
            // CATEGORY
            // -------------------------------------------------

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // -------------------------------------------------
            // FOOD
            // -------------------------------------------------

            modelBuilder.Entity<Food>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.ImageUrl)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);
            });

            // -------------------------------------------------
            // ADDRESS
            // -------------------------------------------------

            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(x => x.Title)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullAddress)
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // -------------------------------------------------
            // CART
            // -------------------------------------------------

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(x => x.User)
                    .WithOne(x => x.Cart)
                    .HasForeignKey<Cart>(
                        x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ProducerProfile)
                    .WithMany(x => x.Carts)
                    .HasForeignKey(
                        x => x.ProducerProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x =>
                        x.RecommendationSearch)
                    .WithMany()
                    .HasForeignKey(x =>
                        x.RecommendationSearchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x =>
                    x.RecommendationSearchId);
            });

            // -------------------------------------------------
            // CART ITEM
            // -------------------------------------------------

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(x => x.Cart)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Food)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.FoodId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.CartId,
                    x.FoodId
                })
                    .IsUnique();
            });

            // -------------------------------------------------
            // FAVORITE
            // -------------------------------------------------

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Favorites)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Food)
                    .WithMany(x => x.Favorites)
                    .HasForeignKey(x => x.FoodId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.FoodId
                })
                    .IsUnique();
            });

            // -------------------------------------------------
            // ORDER
            // -------------------------------------------------

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(x =>
                        x.DeliveryAddressTitle)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x =>
                        x.DeliveryAddress)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x =>
                        x.PaymentMethod)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x =>
                        x.CustomerNote)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x =>
                        x.StatusVersion)
                    .IsConcurrencyToken()
                    .HasDefaultValue(1);

                entity.Property(x => x.TotalPrice)
                    .HasPrecision(18, 2);

                entity.Property(x =>
                        x.SuitabilityScore)
                    .HasPrecision(5, 2);

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x =>
                        x.ProducerProfile)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x =>
                        x.ProducerProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x =>
                        x.RecommendationSearch)
                    .WithMany()
                    .HasForeignKey(x =>
                        x.RecommendationSearchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x =>
                        x.RecommendationSearchId)
                    .IsUnique();
            });

            // -------------------------------------------------
            // ORDER ITEM
            // -------------------------------------------------

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(x => x.FoodName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(x => x.TotalPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Food)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.FoodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------------------------------
            // REVIEW
            // -------------------------------------------------

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(x => x.Comment)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Order)
                    .WithOne(x => x.Review)
                    .HasForeignKey<Review>(
                        x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x =>
                        x.ProducerProfile)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x =>
                        x.ProducerProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------------------------------
            // RECOMMENDATION SEARCH
            // -------------------------------------------------

            modelBuilder.Entity<RecommendationSearch>(
                entity =>
                {
                    entity.Property(x =>
                            x.SearchText)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.HasMany(x =>
                            x.Candidates)
                        .WithOne(x =>
                            x.RecommendationSearch)
                        .HasForeignKey(x =>
                            x.RecommendationSearchId)
                        .OnDelete(
                            DeleteBehavior.Cascade);
                });

            // -------------------------------------------------
            // RECOMMENDATION CANDIDATE
            // -------------------------------------------------

            modelBuilder.Entity<RecommendationCandidate>(
                entity =>
                {
                    entity.Property(x => x.FoodName)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(x =>
                            x.BusinessName)
                        .HasMaxLength(150)
                        .IsRequired();

                    entity.Property(x => x.Price)
                        .HasPrecision(18, 2);

                    entity.Property(x =>
                            x.AverageRating)
                        .HasPrecision(3, 2);

                    entity.HasIndex(x => new
                    {
                        x.RecommendationSearchId,
                        x.Rank
                    });
                });
        }
    }
}