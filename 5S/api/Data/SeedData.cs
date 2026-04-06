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
            //context.Database.Migrate();

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
            else
            {
                var needsUpdate = false;

                if (superAdmin.CompanyId != dummyCompany.Id)
                {
                    superAdmin.CompanyId = dummyCompany.Id;
                    needsUpdate = true;
                }

                if (string.IsNullOrWhiteSpace(superAdmin.Role))
                {
                    superAdmin.Role = "SuperAdmin";
                    needsUpdate = true;
                }

                if (string.IsNullOrWhiteSpace(superAdmin.Name))
                {
                    superAdmin.Name = "Super Admin";
                    needsUpdate = true;
                }

                if (needsUpdate)
                    await userManager.UpdateAsync(superAdmin);

                var validPassword = await userManager.CheckPasswordAsync(superAdmin, "Admin@123");
                if (!validPassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(superAdmin);
                    await userManager.ResetPasswordAsync(superAdmin, token, "Admin@123");
                }
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
            }

            if (!context.ChecklistItems.Any())
            {
                context.ChecklistItems.AddRange(new List<ChecklistItem>
                    {
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Out of order/broken machineries and tools",
                            EvaluationCriteria = "Are out of order machineries and tools removed from the floor or put in quarantine places?",
                            MaxScore = 5,
                            Order = 1
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Unneeded/damaged equipment, files, documents, furniture, bins etc",
                            EvaluationCriteria = "Are unneeded equipment, files, furniture, bins etc. eliminated from the floor?",
                            MaxScore = 5,
                            Order = 2
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Unneeded items on walls, bulletin boards, display etc.",
                            EvaluationCriteria = "Are unused/non-required items on wall, bulletin boards etc. eliminated from the floor?",
                            MaxScore = 5,
                            Order = 3
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Visual Controls",
                            EvaluationCriteria = "Are all the necessary items distinguishable at a glance from a distance?",
                            MaxScore = 5,
                            Order = 4
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Items in aisles, stairways, corners, etc.",
                            EvaluationCriteria = "Are aisles and doorways free from material and blockages (boxes, papers, supplies, etc)?",
                            MaxScore = 5,
                            Order = 5
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Safety hazards (water, oil, chemical, machines)",
                            EvaluationCriteria = "Is there anything that poses a safety threat?",
                            MaxScore = 5,
                            Order = 6
                        },
                        new ChecklistItem
                        {
                            CategoryId = 1,
                            CheckingItemName = "Standards of disposal",
                            EvaluationCriteria = "Are standards of disposal or removal of unwanted/excess items properly followed?",
                            MaxScore = 5,
                            Order = 7
                        },
                        new ChecklistItem
                        {
                            CategoryId = 2,
                            CheckingItemName = "Storage and quarantine places for all materials and equipments",
                            EvaluationCriteria = "Are all the materials and equipments in proper specified positions?",
                            MaxScore = 5,
                            Order = 1
                        },
                        new ChecklistItem
                        {
                            CategoryId = 2,
                            CheckingItemName = "Labels for rack and shelves, Identification of materials",
                            EvaluationCriteria = "Are racks, shelves, and inventory storage areas clearly marked and easy-to-read? Are all the materials and equipments identified?",
                            MaxScore = 5,
                            Order = 2
                        },
                        new ChecklistItem
                        {
                            CategoryId = 2,
                            CheckingItemName = "Proper demarcation and identification marks for all the areas and equipment",
                            EvaluationCriteria = "Are all the areas and equipments properly demarcated?",
                            MaxScore = 5,
                            Order = 3
                        },
                        new ChecklistItem
                        {
                            CategoryId = 2,
                            CheckingItemName = "Dividing lines- In and out for equipments and materials",
                            EvaluationCriteria = "Are all material movement pathways clearly marked (tape on floor/wall or other identification method)?",
                            MaxScore = 5,
                            Order = 4
                        },
                        new ChecklistItem
                        {
                            CategoryId = 2,
                            CheckingItemName = "Documents Management",
                            EvaluationCriteria = "Are there any unlabeled files, folders or documents present? Are obsolete or unused documents routinely separated/removed?",
                            MaxScore = 5,
                            Order = 5
                        },
                        new ChecklistItem
                        {
                            CategoryId = 3,
                            CheckingItemName = "Floor",
                            EvaluationCriteria = "Is the floor clean and free from dust, dirt, oil, ink and other trash?",
                            MaxScore = 5,
                            Order = 1
                        },
                        new ChecklistItem
                        {
                            CategoryId = 3,
                            CheckingItemName = "Equipment and working tables/desks",
                            EvaluationCriteria = "Are equipment clean and free of dirt, oil, and grease? Are working tables and desks tidy?",
                            MaxScore = 5,
                            Order = 2
                        },
                        new ChecklistItem
                        {
                            CategoryId = 3,
                            CheckingItemName = "Machines and tools",
                            EvaluationCriteria = "Are machineries and tools clean and free of dirt, oil, and grease?",
                            MaxScore = 5,
                            Order = 3
                        },
                        new ChecklistItem
                        {
                            CategoryId = 3,
                            CheckingItemName = "Cleaning Responsibilities",
                            EvaluationCriteria = "Is the person responsible for ensuring cleanliness identified and aware of their responsibilities?",
                            MaxScore = 5,
                            Order = 4
                        },
                        new ChecklistItem
                        {
                            CategoryId = 3,
                            CheckingItemName = "Awareness about cleaning",
                            EvaluationCriteria = "Is everyone in the department aware of the importance of cleanliness and practicing it in their surroundings?",
                            MaxScore = 5,
                            Order = 5
                        },
                         new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "Standard work for operations/processes",
                            EvaluationCriteria = "Are standard work methods created and being followed?",
                            MaxScore = 5,
                            Order = 1
                        },
                        new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "Standard work for material movement, cleaning and maintenance",
                            EvaluationCriteria = "Are standard procedures created and being followed?",
                            MaxScore = 5,
                            Order = 2
                        },
                        new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "Understanding and visibility of all necessary standards",
                            EvaluationCriteria = "Are all the standards properly visible and understood by people?",
                            MaxScore = 5,
                            Order = 3
                        },
                        new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "Light & Ventilation",
                            EvaluationCriteria = "Is the intensity of illumination adequate and proper airflow ensured?",
                            MaxScore = 5,
                            Order = 4
                        },
                        new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "Safety clothings and shoes",
                            EvaluationCriteria = "Are all safety measures, including clothing and shoes, properly followed?",
                            MaxScore = 5,
                            Order = 5
                        },
                        new ChecklistItem
                        {
                            CategoryId = 4,
                            CheckingItemName = "The first 3 steps of 5S",
                            EvaluationCriteria = "Is there a system for maintaining clearing up, organizing, and cleaning?",
                            MaxScore = 5,
                            Order = 6
                        },
                        new ChecklistItem
                        {
                            CategoryId = 5,
                            CheckingItemName = "Participation & Involvement",
                            EvaluationCriteria = "Are all the people equally involved with enthusiasm in 5S activities?",
                            MaxScore = 5,
                            Order = 1
                        },
                        new ChecklistItem
                        {
                            CategoryId = 5,
                            CheckingItemName = "5S training for area workers",
                            EvaluationCriteria = "Are all personnel fully trained in the tasks they are responsible for and are they regularly tested?",
                            MaxScore = 5,
                            Order = 2
                        },
                        new ChecklistItem
                        {
                            CategoryId = 5,
                            CheckingItemName = "Daily 5S reviews",
                            EvaluationCriteria = "Are daily reviews happening properly with necessary actions?",
                            MaxScore = 5,
                            Order = 3
                        },
                        new ChecklistItem
                        {
                            CategoryId = 5,
                            CheckingItemName = "Up to date MDI boards, visuals and standard work display",
                            EvaluationCriteria = "Are the MDI boards and visuals properly updated? Are up-to-date work instructions, checklists, and standard systems properly displayed and documented?",
                            MaxScore = 5,
                            Order = 4
                        },
                        new ChecklistItem
                        {
                            CategoryId = 5,
                            CheckingItemName = "Rules and procedures",
                            EvaluationCriteria = "Are all the rules and standard procedures followed by people?",
                            MaxScore = 5,
                            Order = 5
                        }
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}
