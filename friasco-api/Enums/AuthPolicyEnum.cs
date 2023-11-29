namespace friasco_api.Enums;

public static class AuthPolicyEnum
{
    public const string User = nameof(UserRoleEnum.User);
    public const string Admin = nameof(UserRoleEnum.Admin);
    public const string SuperAdmin = nameof(UserRoleEnum.SuperAdmin);
    public const string AdminOrSelf = "AdminOrSelf";
}
