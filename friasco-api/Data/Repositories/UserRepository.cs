using friasco_api.Data.Entities;

namespace friasco_api.Data.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetById(int id);
    Task<User> GetByEmail(string email);
    Task<int> Create(User user);
    Task<int> Update(User user);
    Task<int> Delete(int id);
}

public class UserRepository : IUserRepository
{
    private IDataContext _dataContext;
    private IDapperWrapper _dapperWrapper;
    public UserRepository(IDataContext dataContext, IDapperWrapper dapperWrapper)
    {
        _dataContext = dataContext;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
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
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE id = @id
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, id);
        }
    }

    public async Task<User> GetByEmail(string email)
    {
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM Users
                WHERE Email = @email
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<User>(connection, sql, email);
        }
    }

    public async Task<int> Create(User user)
    {
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
                    PasswordHash
                )
                VALUES 
                (
                    @Username,
                    @FirstName,
                    @LastName,
                    @Email,
                    @Role,
                    @PasswordHash
                )
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, user);
        }
    }

    public async Task<int> Update(User user)
    {
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                UPDATE Users 
                SET Title = @Title,
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
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                DELETE FROM Users 
                WHERE Id = @id
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, id);
        }
    }
}
