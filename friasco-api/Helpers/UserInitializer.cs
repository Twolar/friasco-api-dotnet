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
            var apiTestUser1 = new UserCreateRequestModel
            {
                Username = "apiTestUser1",
                Email = "apiTestUser1@example.com",
                FirstName = "apiTestUser1First",
                LastName = "apiTestUser1Last",
                Role = UserRoleEnum.SuperAdmin,
                Password = "apiTestUser1Password",
                ConfirmPassword = "apiTestUser1Password",
            };
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.Create(apiTestUser1);
                }
            }
            catch (Exception)
            {
                // Do nothing...
            }
        }
    }
}
