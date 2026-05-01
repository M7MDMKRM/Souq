using Microsoft.AspNetCore.Identity;
using Souq.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Souq.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var adminEmail = "mohamed@marketo.com";
            var adminPass = "Marketo@2026";
            var adminRole = "Admin";

            // Create Role if not exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Create User if not exists
            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, adminPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, adminRole);
                }
            }
            else
            {
                // If user exists, ensure password is updated to the new one
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, resetToken, adminPass);

                // Ensure existing admin user has the role
                if (!await userManager.IsInRoleAsync(user, adminRole))
                {
                    await userManager.AddToRoleAsync(user, adminRole);
                }
            }

            // Seed Categories
            var dbContext = serviceProvider.GetRequiredService<SouqcomContext>();

            var categoriesToSeed = new List<Category>
            {
                new Category { Name = "Food & Beverages", Description = "Fresh groceries, snacks, and drinks.", Photo = "https://images.unsplash.com/photo-1542838132-92c53300491e?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Fashion & Clothing", Description = "Latest trends in men's and women's fashion.", Photo = "https://images.unsplash.com/photo-1483985988355-763728e1935b?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Sports & Fitness", Description = "Gear up for your active lifestyle.", Photo = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Electronics & Tech", Description = "Smartphones, laptops, and gadgets.", Photo = "https://images.unsplash.com/photo-1498049794561-7780e7231661?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Home & Decor", Description = "Transform your living space.", Photo = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Health & Beauty", Description = "Premium skincare and wellness products.", Photo = "https://images.unsplash.com/photo-1512290923902-8a9f81dc236c?q=80&w=500&auto=format&fit=crop" },

                new Category { Name = "Automotive", Description = "Car accessories and maintenance tools.", Photo = "https://images.unsplash.com/photo-1485463611174-f302f6a5c1c9?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Books & Stationery", Description = "Expand your knowledge and creativity.", Photo = "https://images.unsplash.com/photo-1456513080510-7bf3a84b82f8?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Pet Supplies", Description = "Everything your furry friends need.", Photo = "https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?q=80&w=500&auto=format&fit=crop" },
                new Category { Name = "Jewelry & Watches", Description = "Elegant accessories for every occasion.", Photo = "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?q=80&w=500&auto=format&fit=crop" }
            };

            foreach (var cat in categoriesToSeed)
            {
                // Strict matching for each category to avoid cross-contamination
                var matches = dbContext.Categories
                    .ToList()
                    .Where(c => c.Name.Trim().Equals(cat.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Special case for "Food" to catch variants if exact match fails
                if (!matches.Any() && cat.Name.Contains("Food"))
                {
                    matches = dbContext.Categories.ToList()
                        .Where(c => c.Name.ToLower().Contains("food"))
                        .ToList();
                }

                if (matches.Any())
                {
                    var main = matches.First();
                    main.Photo = cat.Photo;
                    main.Description = cat.Description;
                    main.Name = cat.Name;

                    if (matches.Count > 1)
                    {
                        dbContext.Categories.RemoveRange(matches.Skip(1));
                    }
                }
                else
                {
                    dbContext.Categories.Add(cat);
                }
            }
            await dbContext.SaveChangesAsync();

            // Seed Specific Featured Products (Matching User Screenshot)
            var featuredProducts = new List<Product>
            {
                new Product { 
                    Name = "Premium Wireless Headphones", 
                    Price = 89.99m, 
                    Description = "High-quality sound with active noise cancellation and 30-hour battery life.",
                    Photo = "https://static.vecteezy.com/system/resources/previews/055/130/517/non_2x/sleek-white-wireless-headphones-premium-audio-bluetooth-noise-cancelling-gear-free-png.png",
                    Catid = dbContext.Categories.FirstOrDefault(c => c.Name.Contains("Electronics"))?.Id ?? 4,
                    IsFeatured = true
                },
                new Product { 
                    Name = "Smart Fitness Watch", 
                    Price = 66.50m, 
                    Description = "Advanced health tracking with heart rate, sleep, and workout monitoring.",
                    Photo = "https://images.pexels.com/photos/12307366/pexels-photo-12307366.jpeg?auto=compress&cs=tinysrgb&dpr=1&w=500",
                    Catid = dbContext.Categories.FirstOrDefault(c => c.Name.Contains("Sports"))?.Id ?? 3,
                    IsFeatured = true
                },
                new Product { 
                    Name = "Classic Canvas Duffle Bag", 
                    Price = 50.60m, 
                    Description = "Durable canvas with genuine leather handles, perfect for getaways.",
                    Photo = "https://img.mytheresa.com/1080/1080/66/jpeg/catalog/product/6e/p01165173_b1.jpg",
                    Catid = dbContext.Categories.FirstOrDefault(c => c.Name.Contains("Fashion"))?.Id ?? 2,
                    IsFeatured = true
                }
            };

            foreach (var fp in featuredProducts)
            {
                if (!dbContext.Products.Any(p => p.Name == fp.Name))
                {
                    dbContext.Products.Add(fp);
                }
            }
            await dbContext.SaveChangesAsync();

            // Seed Remaining Products (10 per category)
            // ONLY seed if the database is completely empty to avoid overwriting your manual changes
            if (dbContext.Products.Count() <= featuredProducts.Count)
            {
                var finalCats = dbContext.Categories.ToList();
                var products = new List<Product>();

                foreach (var cat in finalCats)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        var p = GenerateProduct(cat.Name, i, cat.Id);
                        // Avoid duplicates with featured products
                        if (!featuredProducts.Any(fp => fp.Name == p.Name))
                        {
                            products.Add(p);
                        }
                    }
                }
                dbContext.Products.AddRange(products);
                await dbContext.SaveChangesAsync();
            }
        }

        private static Product GenerateProduct(string categoryName, int index, int catId)
        {
            var p = new Product { Catid = catId };
            
            switch (categoryName)
            {
                case "Fashion & Clothing":
                    p.Name = new[] { "Summer Dress", "Slim Fit Jeans", "Leather Jacket", "Silk Scarf", "Cotton T-Shirt", "Formal Suit", "Winter Coat", "Denim Skirt", "Casual Hoodie", "Linen Shirt" }[index - 1];
                    break;
                case "Electronics & Tech":
                    p.Name = new[] { "Wireless Mouse", "Bluetooth Speaker", "LED Monitor", "Power Bank", "USB-C Cable", "Headphones", "Gaming Keyboard", "Webcam", "Tablet Stand", "Phone Case" }[index - 1];
                    break;
                case "Food & Beverages":
                    p.Name = new[] { "Organic Honey", "Premium Coffee", "Dark Chocolate", "Green Tea", "Olive Oil", "Almond Milk", "Granola Bars", "Pasta Sauce", "Fruit Jam", "Basmati Rice" }[index - 1];
                    break;
                case "Sports & Fitness":
                    p.Name = new[] { "Yoga Mat", "Dumbbell Set", "Running Shoes", "Gym Bag", "Water Bottle", "Smart Watch", "Jump Rope", "Fitness Tracker", "Resistance Bands", "Sport Socks" }[index - 1];
                    break;
                case "Home & Decor":
                    p.Name = new[] { "Scented Candle", "Wall Clock", "Floor Lamp", "Velvet Pillow", "Ceramic Vase", "Throw Blanket", "Picture Frame", "Indoor Plant", "Desk Organizer", "Mirror" }[index - 1];
                    break;
                case "Health & Beauty":
                    p.Name = new[] { "Face Cream", "Shampoo", "Hand Sanitizer", "Lip Balm", "Sunscreen", "Perfume", "Body Wash", "Makeup Kit", "Hair Oil", "Eye Mask" }[index - 1];
                    break;

                case "Automotive":
                    p.Name = new[] { "Car Wax", "Air Freshener", "Seat Cover", "Phone Mount", "Tire Pump", "Dash Cam", "Cleaning Kit", "Tool Box", "Oil Filter", "Jump Starter" }[index - 1];
                    break;
                case "Books & Stationery":
                    p.Name = new[] { "Leather Journal", "Planner 2024", "Gel Pen Set", "Hardcover Novel", "Sketchbook", "Desk Lamp", "Sticky Notes", "Bookmark", "Dictionary", "Highlighters" }[index - 1];
                    break;
                case "Pet Supplies":
                    p.Name = new[] { "Cat Litter", "Dog Bowl", "Pet Bed", "Bird Seed", "Fish Tank Filter", "Pet Shampoo", "Chew Toy", "Leash & Collar", "Grooming Brush", "Treat Bag" }[index - 1];
                    break;
                case "Jewelry & Watches":
                    p.Name = new[] { "Gold Necklace", "Silver Ring", "Leather Watch", "Pearl Earrings", "Bracelet", "Diamond Studs", "Smart Watch", "Cufflinks", "Anklet", "Brooch" }[index - 1];
                    break;
                default:
                    p.Name = $"Product {index}";
                    break;
            }

            p.Price = 20 + index * 10;
            p.Description = $"High-quality {p.Name} designed for professional and daily use.";

            // New Pinterest-inspired High Quality Images (White Background)
            var imageMap = new Dictionary<string, string>
            {
                { "Minimalist Ceramic Vase", "https://static.vecteezy.com/system/resources/previews/013/760/500/non_2x/white-ceramic-vases-decor-without-background-3d-render-png.png" },
                { "Leather Journal", "https://img.freepik.com/free-photo/leather-notebook-isolated-white-background_125540-3331.jpg" },
                { "Organic Honey", "https://static.vecteezy.com/system/resources/previews/060/014/173/large_2x/rustic-jar-of-honey-with-cloth-lid-and-twine-on-white-background-photo.jpeg" },
                { "Gaming Keyboard", "https://static.vecteezy.com/system/resources/previews/052/855/199/non_2x/white-rgb-mechanical-gaming-keyboard-with-cable-free-png.png" },
                { "Ergonomic Office Chair", "https://static.vecteezy.com/system/resources/previews/050/756/184/non_2x/modern-ergonomic-office-chair-design-in-white-and-black-free-png.png" },
                { "Stainless Steel Mug", "https://static.vecteezy.com/system/resources/previews/049/216/072/non_2x/stainless-steel-mug-free-png.png" },
                { "Scented Candle", "https://thumbs.dreamstime.com/b/scented-candle-wooden-wick-glass-jar-white-background-aromatic-clear-isolated-ideal-ambiance-relaxation-376050910.jpg" }
            };

            if (p.Name != null && imageMap.ContainsKey(p.Name))
            {
                p.Photo = imageMap[p.Name];
            }
            else
            {
                string tag = categoryName.Split(' ')[0].ToLower() + ",white-background";
                p.Photo = $"https://loremflickr.com/400/400/{tag}?lock={index}{catId}";
            }

            return p;
        }


    }
}

