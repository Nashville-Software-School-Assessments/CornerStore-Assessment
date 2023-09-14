using CornerStore.Models;
using System.Net.Http.Json;
using System.Net;

namespace CornerStore.Tests;

public class TestCornerStore
{
    [Fact]
    public async void Cashiers()
    {
        var app = new CornerStoreApp();
        var client = app.CreateClient();
        Cashier cashier = await client.GetFromJsonAsync<Cashier>("/cashiers/1");
        // test existing cashier and orders
        Assert.Equal("Amy", cashier.FirstName);
        Assert.Equal("Simpson", cashier.LastName);
        Assert.Equal(2, cashier.Orders.Count);
        var order3 = cashier.Orders.First(o => o.Id == 3);
        var order1 = cashier.Orders.First(o => o.Id == 1);
        Assert.Equal(8.24M, order1.Total);
        Assert.Equal(9.74M, order3.Total);

        var response = await client.PostAsJsonAsync("/cashiers", new Cashier { FirstName = "Test", LastName = "Cashier" });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Cashier>();
        Assert.Equal(4, content.Id);
        Assert.Equal("/cashiers/4", response.Headers.Location.ToString());

    }

    [Fact]
    public async void Products()
    {
        var app = new CornerStoreApp();
        var client = app.CreateClient();
        //Test get products
        var allProducts = await client.GetFromJsonAsync<List<Product>>("/products");
        var cleanProducts = await client.GetFromJsonAsync<List<Product>>("/products?search=clean");
        var tProducts = await client.GetFromJsonAsync<List<Product>>("/products?search=t");
        var vProducts = await client.GetFromJsonAsync<List<Product>>("/products?search=v");
        Assert.Equal(6, allProducts.Count);
        Assert.Equal(2, cleanProducts.Count);
        Assert.Equal(4, tProducts.Count);
        Assert.Equal(1, vProducts.Count);
        Assert.True(allProducts[0].Category != null && allProducts[0].Category.CategoryName != null);

        //create
        var response = await client.PostAsJsonAsync("/products", new Product { ProductName = "Test", CategoryId = 2, Price = 4.11M, Brand = "Test" });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Product>();
        Assert.Equal(7, content.Id);
        Assert.Equal("/products/7", response.Headers.Location.ToString());

        //update
        var updateResponse = await client.PutAsJsonAsync("/products/7", new Product
        {
            Id = 7,
            ProductName = "Testing",
            CategoryId = 2,
            Brand = "Test",
            Price = 4.22M
        });
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        var updatedProducts = await client.GetFromJsonAsync<List<Product>>("/products");
        var updatedProduct = updatedProducts.First(p => p.Id == 7);

        Assert.Equal("Testing", updatedProduct.ProductName);

        //popular - leaving in the assessment as a challenge, but removing from tests for now
        // var popularProducts = await client.GetFromJsonAsync<List<Product>>("/products/popular");

        // Assert.Equal(5, popularProducts.Count);
        // Assert.Equal("Tuna", popularProducts[0].ProductName);

    }

    [Fact]
    public async void Orders()
    {
        var app = new CornerStoreApp();
        var client = app.CreateClient();
        var order2 = await client.GetFromJsonAsync<Order>("/orders/2");

        Assert.Equal(2, order2.Id);
        Assert.Equal(4, order2.OrderProducts.Count);
        Assert.Equal(17.98M, order2.Total);
        var orderTuna = order2.OrderProducts.First(op => op.Product.ProductName == "Tuna");
        Assert.Equal(5, orderTuna.Quantity);

        var allOrders = await client.GetFromJsonAsync<List<Order>>("/orders");
        Assert.Equal(4, allOrders.Count);
        var dateOrders = await client.GetFromJsonAsync<List<Order>>("/orders?orderDate=2023-07-20");
        Assert.Equal(1, dateOrders.Count);

        var deleteResponse = await client.DeleteAsync("/orders/1");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var response = await client.GetAsync("/orders/1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        //create
        var createResponse = await client.PostAsJsonAsync("/orders", new Order
        {
            CashierId = 2,
            PaidOnDate = new DateTime(2023, 7, 24),
            OrderProducts = new()
             {
                new OrderProduct {ProductId = 1, Quantity = 2}
             }
        });
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var content = await createResponse.Content.ReadFromJsonAsync<Order>();
        Assert.Equal(5, content.Id);
        Assert.Equal("/orders/5", createResponse.Headers.Location.ToString());
        Assert.Equal(2.50M, content.Total);
    }
}

//class CornerStoreApp : WebApplicationFactory<Program>
// {
//     protected override IHost CreateHost(IHostBuilder builder)
//     {
//         var root = new InMemoryDatabaseRoot();

//         builder.ConfigureServices(services =>
//         {
//             services.AddScoped(sp =>
//             {
//                 // Replace PostgreSQL with the in memory provider for tests
//                 return new DbContextOptionsBuilder<CornerStoreDbContext>()
//                             .UseInMemoryDatabase("CornerStore", root)
//                             .UseApplicationServiceProvider(sp)
//                             .Options;
//             });

//             var serviceProvider = services.BuildServiceProvider();
//             using (var scope = serviceProvider.CreateScope())
//             {
//                 var context = scope.ServiceProvider.GetRequiredService<CornerStoreDbContext>();
//                 // reset database with testing data
//                 context.Database.EnsureDeleted();
//                 context.Database.EnsureCreated();
//                 context.Cashiers.RemoveRange(context.Cashiers);
//                 context.Categories.RemoveRange(context.Categories);
//                 context.Products.RemoveRange(context.Products);
//                 context.SaveChanges();

//                 var amy = new Cashier { FirstName = "Amy", LastName = "Simpson" };
//                 var derek = new Cashier { FirstName = "Derek", LastName = "Masters" };
//                 var charlie = new Cashier { FirstName = "Charlie", LastName = "Vernon" };
//                 context.Cashiers.AddRange(new Cashier[] { amy, derek, charlie });
//                 context.SaveChanges();

//                 var food = new Category { CategoryName = "Food" };
//                 var cleaning = new Category { CategoryName = "Cleaning" };
//                 var homeImprovement = new Category { CategoryName = "Home Improvement" };
//                 context.Categories.AddRange(new Category[] { food, cleaning, homeImprovement });
//                 context.SaveChanges();

//                 var tuna = new Product { ProductName = "Tuna", Brand = "Bumble Bee", Price = 1.25M, CategoryId = food.Id };
//                 var tomatoes = new Product { ProductName = "Canned Tomatoes", Brand = "Dole", Price = 0.99M, CategoryId = food.Id };
//                 var tp = new Product { ProductName = "Toilet Paper", Brand = "Scott", Price = 5.00M, CategoryId = cleaning.Id };
//                 var dishSoap = new Product { ProductName = "Dishwashing Soap", Brand = "Dawn", Price = 3.75M, CategoryId = cleaning.Id };
//                 var pictureKit = new Product { ProductName = "picture hanging kit", Brand = "Acme", Price = 8.75M, CategoryId = homeImprovement.Id };
//                 var milk = new Product { ProductName = "Milk 2%", Brand = "Dairy", Price = 1.99M, CategoryId = food.Id };
//                 context.Products.AddRange(new Product[] { tuna, tomatoes, tp, dishSoap, pictureKit, milk });
//                 context.SaveChanges();

//                 context.Orders.AddRange(new Order[]
//                 {
//                     new Order
//                     {
//                         CashierId = amy.Id,
//                         PaidOnDate = DateTime.Parse("2023-07-16"),
//                         OrderProducts = new List<OrderProduct>
//                         {
//                             new OrderProduct{ProductId = tuna.Id, Quantity = 1},
//                             new OrderProduct{ProductId = tp.Id, Quantity = 1},
//                             new OrderProduct{ProductId = milk.Id, Quantity = 1}
//                         }
//                     },
//                     new Order
//                     {
//                         CashierId = derek.Id,
//                         PaidOnDate = DateTime.Parse("2023-07-18"),
//                         OrderProducts = new List<OrderProduct>
//                         {
//                             new OrderProduct{ ProductId = tuna.Id, Quantity = 5},
//                             new OrderProduct{ProductId = milk.Id, Quantity = 1},
//                             new OrderProduct {ProductId = pictureKit.Id, Quantity = 1},
//                             new OrderProduct {ProductId = tomatoes.Id, Quantity = 1}
//                         }
//                     },
//                     new Order
//                     {
//                         CashierId = amy.Id,
//                         PaidOnDate = DateTime.Parse("2023-07-20"),
//                         OrderProducts = new List<OrderProduct>
//                         {
//                             new OrderProduct{ProductId = tp.Id, Quantity = 1},
//                             new OrderProduct {ProductId = dishSoap.Id, Quantity = 1},
//                             new OrderProduct {ProductId = tomatoes.Id, Quantity = 1}
//                         }
//                     },
//                     new Order
//                     {
//                         CashierId = charlie.Id,
//                         PaidOnDate = DateTime.Parse("2023-07-13"),
//                         OrderProducts = new List<OrderProduct>
//                         {
//                             new OrderProduct { ProductId = tomatoes.Id, Quantity = 1}
//                         }
//                     },
//                 });
//                 context.SaveChanges();

//             }
//         });

//         return base.CreateHost(builder);
//     }
// }
