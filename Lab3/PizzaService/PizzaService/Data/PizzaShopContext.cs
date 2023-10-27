using Microsoft.EntityFrameworkCore;
using PizzaShopService.Models;

namespace PizzaShopService.Data
{
	public class PizzaShopContext : DbContext
	{
		public DbSet<Customer> Customers { get; set; } = null!;
		public DbSet<Order> Orders { get; set; } = null!;
		public DbSet<Product> Products { get; set; } = null!;
		public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=PizzaShop");
		}
	}
}
