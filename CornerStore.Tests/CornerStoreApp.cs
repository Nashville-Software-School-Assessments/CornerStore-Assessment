using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CornerStore.Models;

namespace CornerStore.Tests;

public class CornerStoreApp : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.AddScoped(sp =>
            {
                // Replace PostgreSQL with the in memory provider for tests
                return new DbContextOptionsBuilder<CornerStoreDbContext>()
                            .UseInMemoryDatabase("CornerStore", root)
                            .UseApplicationServiceProvider(sp)
                            .Options;
            });

            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CornerStoreDbContext>();
                // reset database with testing data
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Cashiers.RemoveRange(context.Cashiers);
                context.Categories.RemoveRange(context.Categories);
                context.Products.RemoveRange(context.Products);
                context.Orders.RemoveRange(context.Orders);
                context.OrderProducts.RemoveRange(context.OrderProducts);
                context.SaveChanges();

                var amy = new Cashier { FirstName = "Amy", LastName = "Simpson" };
                var derek = new Cashier { FirstName = "Derek", LastName = "Masters" };
                var charlie = new Cashier { FirstName = "Charlie", LastName = "Vernon" };
                context.Cashiers.AddRange(new Cashier[] { amy, derek, charlie });
                context.SaveChanges();

                var food = new Category { CategoryName = "Food" };
                var cleaning = new Category { CategoryName = "Cleaning" };
                var homeImprovement = new Category { CategoryName = "Home Improvement" };
                context.Categories.AddRange(new Category[] { food, cleaning, homeImprovement });
                context.SaveChanges();

                var tuna = new Product { ProductName = "Tuna", Brand = "Bumble Bee", Price = 1.25M, CategoryId = food.Id };
                var tomatoes = new Product { ProductName = "Canned Tomatoes", Brand = "Dole", Price = 0.99M, CategoryId = food.Id };
                var tp = new Product { ProductName = "Toilet Paper", Brand = "Scott", Price = 5.00M, CategoryId = cleaning.Id };
                var dishSoap = new Product { ProductName = "Dishwashing Soap", Brand = "Dawn", Price = 3.75M, CategoryId = cleaning.Id };
                var pictureKit = new Product { ProductName = "picture hanging kit", Brand = "Acme", Price = 8.75M, CategoryId = homeImprovement.Id };
                var milk = new Product { ProductName = "Milk 2%", Brand = "Dairy", Price = 1.99M, CategoryId = food.Id };
                context.Products.AddRange(new Product[] { tuna, tomatoes, tp, dishSoap, pictureKit, milk });
                context.SaveChanges();

                context.Orders.AddRange(new Order[]
                {
                    new Order
                    {
                        CashierId = amy.Id,
                        PaidOnDate = DateTime.Parse("2023-07-16"),
                        OrderProducts = new List<OrderProduct>
                        {
                            new OrderProduct{ProductId = tuna.Id, Quantity = 1},
                            new OrderProduct{ProductId = tp.Id, Quantity = 1},
                            new OrderProduct{ProductId = milk.Id, Quantity = 1}
                        }
                    },
                    new Order
                    {
                        CashierId = derek.Id,
                        PaidOnDate = DateTime.Parse("2023-07-18"),
                        OrderProducts = new List<OrderProduct>
                        {
                            new OrderProduct{ ProductId = tuna.Id, Quantity = 5},
                            new OrderProduct{ProductId = milk.Id, Quantity = 1},
                            new OrderProduct {ProductId = pictureKit.Id, Quantity = 1},
                            new OrderProduct {ProductId = tomatoes.Id, Quantity = 1}
                        }
                    },
                    new Order
                    {
                        CashierId = amy.Id,
                        PaidOnDate = DateTime.Parse("2023-07-20"),
                        OrderProducts = new List<OrderProduct>
                        {
                            new OrderProduct{ProductId = tp.Id, Quantity = 1},
                            new OrderProduct {ProductId = dishSoap.Id, Quantity = 1},
                            new OrderProduct {ProductId = tomatoes.Id, Quantity = 1}
                        }
                    },
                    new Order
                    {
                        CashierId = charlie.Id,
                        PaidOnDate = DateTime.Parse("2023-07-13"),
                        OrderProducts = new List<OrderProduct>
                        {
                            new OrderProduct { ProductId = tomatoes.Id, Quantity = 1}
                        }
                    },
                });
                context.SaveChanges();

            }
        });

        return base.CreateHost(builder);
    }
}