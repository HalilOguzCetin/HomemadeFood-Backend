using HomemadeFood.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<ProducerProfile> ProducerProfiles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Food> Foods { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(x => x.Comment)
                    .HasMaxLength(1000)
                    .IsRequired();
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(x => x.FoodName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(x => x.TotalPrice)
                    .HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(x => x.DeliveryAddressTitle)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DeliveryAddress)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.PaymentMethod)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.CustomerNote)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.TotalPrice)
                    .HasPrecision(18, 2);

                entity.Property(x => x.SuitabilityScore)
                    .HasPrecision(5, 2);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(x => x.Title)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.FullAddress)
                    .HasMaxLength(500)
                    .IsRequired();
            });

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

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500)
                    .IsRequired();
            });

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

                entity.Property(x => x.Rating)
                    .HasPrecision(3, 2);
                entity.Property(x => x.CapacityDate)
                    .HasColumnType("date");
                entity.Property(x => x.CapacityVersion)
    .IsConcurrencyToken()
    .HasDefaultValue(1);


            });


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
            });

            modelBuilder.Entity<User>()
                .HasOne(u => u.ProducerProfile)
                .WithOne(p => p.User)
                .HasForeignKey<ProducerProfile>(p => p.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
    .HasOne(r => r.Order)
    .WithOne(o => o.Review)
    .HasForeignKey<Review>(r => r.OrderId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.ProducerProfile)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProducerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Favorite>()
    .HasOne(f => f.User)
    .WithMany(u => u.Favorites)
    .HasForeignKey(f => f.UserId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Food)
                .WithMany(food => food.Favorites)
                .HasForeignKey(f => f.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new
                {
                    f.UserId,
                    f.FoodId
                })
                .IsUnique();
            modelBuilder.Entity<Cart>()
    .HasOne(c => c.User)
    .WithOne(u => u.Cart)
    .HasForeignKey<Cart>(c => c.UserId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.ProducerProfile)
                .WithMany(p => p.Carts)
                .HasForeignKey(c => c.ProducerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Food)
                .WithMany(f => f.CartItems)
                .HasForeignKey(ci => ci.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new
                {
                    ci.CartId,
                    ci.FoodId
                })
                .IsUnique();
            modelBuilder.Entity<Order>()
    .HasOne(o => o.ProducerProfile)
    .WithMany(p => p.Orders)
    .HasForeignKey(o => o.ProducerProfileId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Food)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(oi => oi.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}