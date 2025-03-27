using ASM_C_5.Areas.Identity.Data;
using ASM_C_5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ASM_C_5.Data;

public class ASM_C_5Context : IdentityDbContext<ApplicationUser>
{
    public ASM_C_5Context(DbContextOptions<ASM_C_5Context> options)
        : base(options)
    {
    }
    public DbSet<FoodCategory> FoodCategories { get; set; }
    public DbSet<FoodItem> FoodItems { get; set; }
    public DbSet<ComboDetail> ComboDetails { get; set; }

    public DbSet<ComboItem> ComboItems { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.CreatedBy).HasMaxLength(255);
            entity.Property(r => r.UpdatedBy).HasMaxLength(255);
            entity.Property(r => r.CreatedDate).IsRequired();
        });

        // Cấu hình FoodItem với FoodCategory (1-N)
        builder.Entity<FoodItem>()
            .HasOne(f => f.Category)
            .WithMany()
            .HasForeignKey(f => f.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình bảng trung gian ComboDetail (N-N)
        builder.Entity<ComboDetail>()
            .HasKey(cd => new { cd.FoodID, cd.ComboID });

        builder.Entity<ComboDetail>()
            .HasOne(cd => cd.Food)
            .WithMany()
            .HasForeignKey(cd => cd.FoodID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ComboDetail>()
            .HasOne(cd => cd.Combo)
            .WithMany()
            .HasForeignKey(cd => cd.ComboID)
            .OnDelete(DeleteBehavior.Cascade);

        // Cấu hình bảng OrderDetail
        builder.Entity<OrderDetail>()
    .HasKey(od => od.OrderDetailID);

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Food)
            .WithMany()
            .HasForeignKey(od => od.FoodID)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // Có thể null

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Combo)
            .WithMany()
            .HasForeignKey(od => od.ComboID)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // Có thể null

        // **Cấu hình Cart liên kết với ApplicationUser**
        builder.Entity<Cart>()
            .HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // **Cấu hình Order liên kết với ApplicationUser**
        builder.Entity<Order>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<ASM_C_5.Models.ApplicationRole> ApplicationRole { get; set; } = default!;
}
