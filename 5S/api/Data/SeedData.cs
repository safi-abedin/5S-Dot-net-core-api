using api.Models.Checklists;
using api.Models.Companies;
using api.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Apply migrations
            context.Database.Migrate();

            // 🏢 1. DUMMY COMPANY
            var dummyCompany = await context.Companies.FirstOrDefaultAsync(c => c.CompanyCode == "DUMMY_COMPANY");
            if (dummyCompany == null)
            {
                dummyCompany = new Company
                {
                    CompanyName = "Dummy Company",
                    CompanyCode = "DUMMY_COMPANY",
                    ContactPerson = "Super Admin",
                    Email = "dummy@company.com",
                    Phone = "0000000000"
                };

                context.Companies.Add(dummyCompany);
                await context.SaveChangesAsync();
            }

            // 🔐 2. SUPER ADMIN
            var superAdmin = await userManager.FindByNameAsync("superadmin");

            if (superAdmin == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "superadmin",
                    Name = "Super Admin",
                    Role = "SuperAdmin",
                    CompanyId = dummyCompany.Id,
                };

                await userManager.CreateAsync(user, "Admin@123");
            }
            else if (superAdmin.CompanyId != dummyCompany.Id)
            {
                superAdmin.CompanyId = dummyCompany.Id;
                await userManager.UpdateAsync(superAdmin);
            }

            // 📋 3. CHECKLIST SEED
            if (!context.ChecklistCategories.Any())
            {
                var categories = new List<ChecklistCategory>
            {
                new ChecklistCategory { Name = "Sort", Order = 1 },
                new ChecklistCategory { Name = "Set in Order", Order = 2 },
                new ChecklistCategory { Name = "Shine", Order = 3 },
                new ChecklistCategory { Name = "Standardize", Order = 4 },
                new ChecklistCategory { Name = "Sustain", Order = 5 }
            };

                context.ChecklistCategories.AddRange(categories);
                await context.SaveChangesAsync();

                // Example Items
                context.ChecklistItems.AddRange(new List<ChecklistItem>
            {
                new ChecklistItem { CategoryId = categories[0].Id, QuestionText = "Unnecessary items removed?", Order = 1 },
                new ChecklistItem { CategoryId = categories[0].Id, QuestionText = "Red tag area available?", Order = 2 },

                new ChecklistItem { CategoryId = categories[1].Id, QuestionText = "Proper storage maintained?", Order = 1 }
            });

                await context.SaveChangesAsync();
            }
        }
    }
}
