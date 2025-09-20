using RupalStudentCore8App.Server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static RupalStudentCore8App.Server.Class.GlobalConstant;

namespace RupalStudentCore8App.Server.Data
{
    public class IdentityDataInitializer
    {
        private readonly ILogger<IdentityDataInitializer> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<AspNetRole> _roleManager;

        public IdentityDataInitializer(
            ApplicationDbContext dbContext,
            UserManager<AspNetUser> userManager,
            RoleManager<AspNetRole> roleManager,
            ILogger<IdentityDataInitializer> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public class InitUser : AspNetUser
        {
            public string Password { get; set; }
            public string Role { get; set; }
        }
        public async Task InitializeAsync()
        {
            try
            {
                var roleList = new List<AspNetRole>
                {
                    new AspNetRole { Name = RoleType.Administrator, ReadOnly = true },
                    new AspNetRole { Name = RoleType.Admin, ReadOnly = true }
                };

                var userList = new List<InitUser>
                {
                    new InitUser { Role = RoleType.Administrator, FullName = "Administrator", UserName = "administrator", Email = "administrator@thebluchip.com", Password = "Asdfg@12#", Readonly = true },
                    new InitUser { Role = RoleType.Admin, FullName = "Admin", UserName = "admin", Email = "admin@thebluchip.com", Password = "Asdfg@12#", Readonly = true }
                };

                await CreateRolesAsync(roleList);
                await CreateUsersAsync(userList);
                await CreateMenuAndAssignPermissionAsync();

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during database initialization", ex);
                throw;
            }
        }
        private async Task CreateRolesAsync(List<AspNetRole> roleList)
        {
            foreach (var role in roleList)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role.Name);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to create role {RoleName}: {Errors}", role.Name, errors);
                        throw new InvalidOperationException($"Failed to create role {role.Name}");
                    }
                }
            }
        }

        private async Task CreateUsersAsync(List<InitUser> userList)
        {
            foreach (var _user in userList)
            {
                AspNetUser? user = null;
                var existingUser = await _userManager.FindByNameAsync(_user.UserName);
                if (existingUser == null)
                {
                    user = new AspNetUser
                    {
                        FullName = _user.FullName,
                        UserName = _user.UserName,
                        Email = _user.Email,
                        EmailConfirmed = true,
                        Status = true,
                        Readonly = _user.Readonly,
                    };

                    var result = await _userManager.CreateAsync(user, _user.Password);
                    if (result.Succeeded)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, _user.Role);
                        if (!roleResult.Succeeded)
                        {
                            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            _logger.LogError("Failed to add user {UserName} to role {Role}: {Errors}", user.UserName, _user.Role, errors);
                            throw new InvalidOperationException($"Failed to add user {user.UserName} to role {_user.Role}");
                        }
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to create user {UserName}: {Errors}", user.UserName, errors);
                        throw new InvalidOperationException($"Failed to create user {user.UserName}");
                    }
                }
            }
        }

        private async Task CreateMenuAndAssignPermissionAsync()
        {

            try
            {
                using (var db = _dbContext)
                {

                    // Create dashboard menu if not exists
                    var dashboardExists = await db.AspMenus.AnyAsync(w => w.Name == "Dashboard");
                    if (!dashboardExists)
                    {
                        await db.AspMenus.AddAsync(new AspMenu
                        {
                            Name = "Dashboard",
                            Title = "Dashboard",
                            TitleAr = "Dashboard",
                            Icon = "ft-home",
                            Path = "/dashboard",
                            ParentId = 0,
                            OrderNo = 1,
                            Class = "",
                            IsExternalLink = false
                        });
                    }
                    await db.SaveChangesAsync();

                    // Get administrator role
                    var adminRole = await _roleManager.FindByNameAsync(RoleType.Administrator);
                    if (adminRole == null)
                    {
                        _logger.LogWarning("Administrator role not found");
                        return;
                    }

                    // Get all existing menus
                    var existingMenus = await db.AspMenus.ToListAsync();

                    // Create permissions in bulk
                    var existingPermissionMenuIds = await db.AspMenuPermissions
                        .Where(p => p.RoleId == adminRole.Id)
                        .Select(p => p.MenuId)
                        .ToListAsync();

                    var existingPermissionMenuIdsSet = new HashSet<int>(existingPermissionMenuIds.Where(id => id.HasValue).Select(id => id.Value));

                    var newPermissions = existingMenus
                        .Where(menu => !existingPermissionMenuIdsSet.Contains(menu.Id))
                        .Select(menu => new AspMenuPermission
                        {
                            MenuId = menu.Id,
                            RoleId = adminRole.Id
                        })
                        .ToList();

                    if (newPermissions.Any())
                    {
                        await db.AspMenuPermissions.AddRangeAsync(newPermissions);
                    }

                    await db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seed menu and permissions");
                throw; // Re-throw to ensure caller knows operation failed
            }
        }

    }
}
