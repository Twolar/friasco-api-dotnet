using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
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
    private readonly ILogger<IUserService> _logger;
    private IUserRepository _userRepository;

    public UserService(ILogger<IUserService> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        _logger.LogDebug("UserService::GetAll");
        return await _userRepository.GetAll();
    }

    public async Task<User> GetById(int id)
    {
        _logger.LogDebug($"UserService::GetById id: {id}");

        var user = await _userRepository.GetById(id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id: {id} not found");
        }

        return user;
    }

    public async Task<int> Create(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "UserService::Create");

        // TODO: Verify Duplicate Email
        // TODO: Map Request Model to Data Model
        // TODO: Password Hashing

        throw new NotImplementedException();
    }

    public async Task<int> Update(int id, UserUpdateRequestModel model)
    {
        _logger.LogDebug($"UserService::Update id: {id}");

        // TODO: User exists
        // TODO: Validate if email is an incoming change
        // TODO: Validate if password is an incoming change / Hash it
        // TODO: Model mapping

        throw new NotImplementedException();
    }

    public async Task<int> Delete(int id)
    {
        _logger.LogDebug($"UserService::Delete id: {id}");
        return await _userRepository.Delete(id);
    }
}
