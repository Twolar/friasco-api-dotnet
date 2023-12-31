﻿using friasco_api.Data.Entities;

namespace friasco_api.Data.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetById(int id);
    Task<User> GetByGuid(Guid userGuid);
    Task<User> GetByEmail(string email);
    Task<User> GetByUsername(string username);
    Task<int> Create(User user);
    Task<int> Update(User user);
    Task<int> Delete(int id);
}

public class UserRepository : IUserRepository
{
    private readonly ILogger<IUserRepository> _logger;
    private IDataContext _dataContext;
    private IDapperWrapper _dapperWrapper;
    public UserRepository(ILogger<IUserRepository> logger, IDataContext dataContext, IDapperWrapper dapperWrapper)
    {
        _logger = logger;
        _dataContext = dataContext;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        _logger.LogDebug("UserRepository::GetAll");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
            ";
            return await _dapperWrapper.QueryAsync<User>(connection, sql);
        }
    }

    public async Task<User> GetById(int id)
    {
        _logger.LogDebug($"UserRepository::GetById id: {id}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE id = @id
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, new { id });
        }
    }

    public async Task<User> GetByGuid(Guid userGuid)
    {
        _logger.LogDebug($"UserRepository::GetByGuid userGuid: {userGuid}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE Guid = @userGuid
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, new { userGuid });
        }
    }

    public async Task<User> GetByEmail(string email)
    {
        _logger.LogDebug($"UserRepository::GetByEmail email: {email}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE Email = @email
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, new { email });
        }
    }

    public async Task<User> GetByUsername(string username)
    {
        _logger.LogDebug($"UserRepository::GetByUsername username: {username}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE Username = @username
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, new { username });
        }
    }

    public async Task<int> Create(User user)
    {
        _logger.Log(LogLevel.Debug, "UserRepository::Create");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                INSERT INTO Users 
                (
                    Username,
                    FirstName,
                    LastName,
                    Email,
                    Role,
                    PasswordHash,
                    Guid
                )
                VALUES 
                (
                    @Username,
                    @FirstName,
                    @LastName,
                    @Email,
                    @Role,
                    @PasswordHash,
                    @Guid
                )
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, user);
        }
    }

    public async Task<int> Update(User user)
    {
        _logger.LogDebug($"UserRepository::Update id: {user.Id}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                UPDATE Users 
                SET Username = @Username,
                    FirstName = @FirstName,
                    LastName = @LastName, 
                    Email = @Email, 
                    Role = @Role, 
                    PasswordHash = @PasswordHash
                WHERE Id = @Id
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, user);
        }
    }

    public async Task<int> Delete(int id)
    {
        _logger.LogDebug($"UserRepository::Delete id: {id}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                DELETE FROM Users 
                WHERE Id = @id
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, new { id });
        }
    }
}
