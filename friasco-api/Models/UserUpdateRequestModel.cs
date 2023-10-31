using System.ComponentModel.DataAnnotations;
using friasco_api.Enums;

namespace friasco_api.Models;

public class UserUpdateRequestModel
{
    public string? Username { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [EnumDataType(typeof(UserRoleEnum))]
    public UserRoleEnum? Role { get; set; }

    private string? _password;
    [MinLength(6)]
    public string? Password
    {
        get => _password;
        set
        {
            _password = ReplaceEmptyWithNull(value);
        }
    }

    private string? _confirmPassword;
    [Compare("Password")]
    public string? ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = ReplaceEmptyWithNull(value);
        }
    }

    #region Helpers

    private string? ReplaceEmptyWithNull(string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    #endregion
}
