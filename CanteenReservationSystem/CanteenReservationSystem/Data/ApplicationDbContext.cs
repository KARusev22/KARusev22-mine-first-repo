using CanteenReservationSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Nutrition> Nutritions { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Polls> Polls { get; set; }
        public DbSet<PollOptions> PollOptions { get; set; }
        public DbSet<Votes> Votes { get; set; }
        public DbSet<MonthlyMenu> MonthlyMenu { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<DishAllergen>()
                .HasKey(da => new { da.DishId, da.AllergenId });

            builder.Entity<DishIngredient>()
                .HasKey(di => new { di.DishId, di.IngredientId });
            
            builder.Entity<Nutrition>()
                .HasOne(n => n.Dish)
                .WithOne(d => d.Nutrition)
                .HasForeignKey<Nutrition>(n => n.DishId);
        }
    }
}