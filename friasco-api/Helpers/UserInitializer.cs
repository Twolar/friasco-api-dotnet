using friasco_api.Enums;
using friasco_api.Models;
using friasco_api.Services;

namespace friasco_api.Helpers;

public static class UserInitializer
{
    public static async Task CreateTestUsers(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // TODO: When eventually in a production environment check that this user is not being setup.
            var userWithUserRole = new UserCreateRequestModel
            {
                Username = "UserRole",
                Email = "UserRole@example.com",
                FirstName = "UserRoleFirst",
                LastName = "UserRoleLast",
                Role = UserRoleEnum.User,
                Password = "Password123",
                ConfirmPassword = "Password123",
            };
            var userWithAdminRole = new UserCreateRequestModel
            {
                Username = "AdminRole",
                Email = "Admin@example.com",
                FirstName = "AdminRoleFirst",
                LastName = "AdminRoleLast",
                Role = UserRoleEnum.Admin,
                Password = "Password123",
                ConfirmPassword = "Password123",
            };
            var userWithSuperAdminRole = new UserCreateRequestModel
            {
                Username = "SuperAdminRole",
                Email = "SuperAdminRole@example.com",
                FirstName = "SuperAdminRoleFirst",
                LastName = "SuperAdminRoleLast",
                Role = UserRoleEnum.SuperAdmin,
                Password = "Password123",
                ConfirmPassword = "Password123",
            };

            using (var scope = app.Services.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                try
                {
                    await userService.Create(userWithUserRole);
                }
                catch (Exception) { }

                try
                {
                    await userService.Create(userWithAdminRole);
                }
                catch (Exception) { }

                try
                {
                    await userService.Create(userWithSuperAdminRole);
                }
                catch (Exception) { }
            }
        }
    }
}
