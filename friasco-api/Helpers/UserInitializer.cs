using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;

namespace friasco_api.Helpers;

public static class UserInitializer
{
    public static async Task CreateTestUsers(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // TODO: When eventually in a production environment check that these users are not being setup.
            var userWithUserRole = new User
            {
                Username = "UserRole",
                Email = "UserRole@example.com",
                FirstName = "UserRoleFirst",
                LastName = "UserRoleLast",
                Role = UserRoleEnum.User,
                Guid = Guid.NewGuid()
            };
            var userWithAdminRole = new User
            {
                Username = "AdminRole",
                Email = "AdminRole@example.com",
                FirstName = "AdminRoleFirst",
                LastName = "AdminRoleLast",
                Role = UserRoleEnum.Admin,
                Guid = Guid.NewGuid()
            };
            var userWithSuperAdminRole = new User
            {
                Username = "SuperAdminRole",
                Email = "SuperAdminRole@example.com",
                FirstName = "SuperAdminRoleFirst",
                LastName = "SuperAdminRoleLast",
                Role = UserRoleEnum.SuperAdmin,
                Guid = Guid.NewGuid()
            };

            using (var scope = app.Services.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var bcryptWrapper = scope.ServiceProvider.GetRequiredService<IBCryptWrapper>();

                var passwordForAllUsers = "Password123";

                try
                {
                    if (await userRepository.GetByEmail(userWithUserRole.Email!) == null)
                    {
                        userWithUserRole.PasswordHash = bcryptWrapper.HashPassword(passwordForAllUsers);
                        await userRepository.Create(userWithUserRole);
                    }
                }
                catch (Exception)
                {
                    // Do nothing...
                }

                try
                {
                    if (await userRepository.GetByEmail(userWithAdminRole.Email!) == null)
                    {
                        userWithAdminRole.PasswordHash = bcryptWrapper.HashPassword(passwordForAllUsers);
                        await userRepository.Create(userWithAdminRole);
                    }
                }
                catch (Exception)
                {
                    // Do nothing...
                }

                try
                {
                    if (await userRepository.GetByEmail(userWithSuperAdminRole.Email!) == null)
                    {
                        userWithSuperAdminRole.PasswordHash = bcryptWrapper.HashPassword(passwordForAllUsers);
                        await userRepository.Create(userWithSuperAdminRole);
                    }
                }
                catch (Exception)
                {
                    // Do nothing...}
                }
            }
        }
    }
}
