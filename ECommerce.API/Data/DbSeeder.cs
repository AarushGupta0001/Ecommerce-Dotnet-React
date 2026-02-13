using ECommerce.API.Helpers;
using ECommerce.API.Models;

namespace ECommerce.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Users.Any(u => u.Email == "admin@test.com"))
            {
                context.Users.Add(new User
                {
                    Name = "Admin",
                    Email = "admin@test.com",
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    Role = "Admin"
                });
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Name = "iPhone 14",
                        Description = "Apple smartphone",
                        Price = 79999,
                        Stock = 10,
                        ImageUrl = "https://via.placeholder.com/150"
                    },
                    new Product
                    {
                        Name = "Nike Shoes",
                        Description = "Running shoes",
                        Price = 4999,
                        Stock = 25,
                        ImageUrl = "https://via.placeholder.com/150"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
