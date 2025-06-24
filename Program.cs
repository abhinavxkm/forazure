using Microsoft.EntityFrameworkCore;
using EasyHousingSolution.Models;
using BCrypt.Net;

namespace EasyHousingSolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession();

            var app = builder.Build();

            // --- Seeding ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                if (!context.Users.Any())
                {
                    Console.WriteLine("Database is empty. Seeding initial data...");

                    // 1. States
                    var stateMH = new State { StateName = "Maharashtra" };
                    var stateKA = new State { StateName = "Karnataka" };
                    var stateTN = new State { StateName = "Tamil Nadu" };
                    context.States.AddRange(stateMH, stateKA, stateTN);
                    context.SaveChanges();

                    // 2. Cities
                    var cityPune = new City { CityName = "Pune", StateId = stateMH.StateId };
                    var cityMumbai = new City { CityName = "Mumbai", StateId = stateMH.StateId };
                    var cityBengaluru = new City { CityName = "Bengaluru", StateId = stateKA.StateId };
                    var cityChennai = new City { CityName = "Chennai", StateId = stateTN.StateId };
                    context.Cities.AddRange(cityPune, cityMumbai, cityBengaluru, cityChennai);
                    context.SaveChanges();

                    // 3. Users only (hash passwords here)
                    context.Users.AddRange(
                        new User { UserName = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin"), UserType = "Admin" },
                        new User { UserName = "seller1", Password = BCrypt.Net.BCrypt.HashPassword("seller1"), UserType = "Seller" },
                        new User { UserName = "seller2", Password = BCrypt.Net.BCrypt.HashPassword("seller2"), UserType = "Seller" },
                        new User { UserName = "seller3", Password = BCrypt.Net.BCrypt.HashPassword("seller3"), UserType = "Seller" },
                        new User { UserName = "buyer1", Password = BCrypt.Net.BCrypt.HashPassword("buyer1"), UserType = "Buyer" },
                        new User { UserName = "buyer2", Password = BCrypt.Net.BCrypt.HashPassword("buyer2"), UserType = "Buyer" },
                        new User { UserName = "buyer3", Password = BCrypt.Net.BCrypt.HashPassword("buyer3"), UserType = "Buyer" }
                    );
                    context.SaveChanges(); // Save Users FIRST

                    // 4. Sellers
                    var seller1 = new Seller
                    {
                        UserName = "seller1",
                        FirstName = "John",
                        LastName = "Doe",
                        DateOfBirth = new DateTime(1985, 5, 20),
                        PhoneNo = "9876543210",
                        EmailId = "john.d@example.com",
                        Address = "123 Maple Street",
                        StateId = stateMH.StateId,
                        CityId = cityPune.CityId
                    };

                    var seller2 = new Seller
                    {
                        UserName = "seller2",
                        FirstName = "Lisa",
                        LastName = "Ray",
                        DateOfBirth = new DateTime(1991, 8, 15),
                        PhoneNo = "8765432109",
                        EmailId = "lisa.r@example.com",
                        Address = "202 Oak Avenue",
                        StateId = stateKA.StateId,
                        CityId = cityBengaluru.CityId
                    };

                    var seller3 = new Seller
                    {
                        UserName = "seller3",
                        FirstName = "Peter",
                        LastName = "Jones",
                        DateOfBirth = new DateTime(1988, 3, 12),
                        PhoneNo = "7654321098",
                        EmailId = "peter.j@example.com",
                        Address = "303 Pine Lane",
                        StateId = stateTN.StateId,
                        CityId = cityChennai.CityId
                    };

                    context.Sellers.AddRange(seller1, seller2, seller3);

                    // 5. Buyers
                    context.Buyers.AddRange(
                        new Buyer { UserName = "buyer1", FirstName = "Mike", LastName = "Ross", DateOfBirth = new DateTime(1992, 11, 30), PhoneNo = "6543210987", EmailId = "mike.r@example.com" },
                        new Buyer { UserName = "buyer2", FirstName = "Sara", LastName = "Lee", DateOfBirth = new DateTime(1995, 2, 10), PhoneNo = "5432109876", EmailId = "sara.l@example.com" },
                        new Buyer { UserName = "buyer3", FirstName = "Tom", LastName = "King", DateOfBirth = new DateTime(1993, 7, 22), PhoneNo = "4321098765", EmailId = "tom.k@example.com" }
                    );

                    context.SaveChanges(); // Save Sellers and Buyers

                    // 6. Properties
                    context.Properties.AddRange(
                        new Property
                        {
                            PropertyName = "Modern 2BHK Apartment",
                            PropertyType = "Apartment",
                            PropertyOption = "Rent",
                            Description = "A beautiful, modern 2BHK apartment in the heart of the city with all amenities.",
                            Address = "Koregaon Park, Pune",
                            Region = "Pune",
                            PriceRange = 25000.00m,
                            InitialDeposit = 100000.00m,
                            Landmark = "Near Osho Garden",
                            IsActive = true,
                            SellerId = seller1.SellerId
                        },
                        new Property
                        {
                            PropertyName = "Spacious 3BHK Villa",
                            PropertyType = "Villa",
                            PropertyOption = "Sell",
                            Description = "Luxurious villa with a private garden and swimming pool. Perfect for families.",
                            Address = "Indiranagar, Bengaluru",
                            Region = "Bengaluru",
                            PriceRange = 25000000.00m,
                            InitialDeposit = 0,
                            Landmark = "100ft Road",
                            IsActive = true,
                            SellerId = seller2.SellerId
                        },
                        new Property
                        {
                            PropertyName = "Sea View Flat",
                            PropertyType = "Flat",
                            PropertyOption = "Sell",
                            Description = "Stunning flat with an uninterrupted view of the sea. Fully furnished.",
                            Address = "Besant Nagar, Chennai",
                            Region = "Chennai",
                            PriceRange = 18000000.00m,
                            InitialDeposit = 0,
                            Landmark = "Near Elliot's Beach",
                            IsActive = false,
                            SellerId = seller3.SellerId
                        },
                        new Property
                        {
                            PropertyName = "Cozy 1RK Studio",
                            PropertyType = "Studio",
                            PropertyOption = "Rent",
                            Description = "A compact and cozy studio apartment, ideal for students or bachelors.",
                            Address = "Hinjawadi, Pune",
                            Region = "Pune",
                            PriceRange = 12000.00m,
                            InitialDeposit = 50000.00m,
                            Landmark = "Phase 1 IT Park",
                            IsActive = true,
                            SellerId = seller1.SellerId
                        }
                    );

                    context.SaveChanges();
                    Console.WriteLine("Initial data including properties seeded successfully.");
                }
            }

            // --- Configure HTTP Pipeline ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
