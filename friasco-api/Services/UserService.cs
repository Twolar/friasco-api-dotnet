using friasco_api.Data.Entities;
using friasco_api.Models;

namespace friasco_api.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetById(int id);
    Task<int> Create(UserCreateRequestModel model);
    Task<int> Update(int id, UserUpdateRequestModel model);
    Task<int> Delete(int id);
}

public class UserService : IUserService
{

}
