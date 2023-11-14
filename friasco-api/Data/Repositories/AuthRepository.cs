
using friasco_api.Data.Entities;

namespace friasco_api.Data.Repositories;

public interface IAuthRepository
{
    Task<int> CreateRefreshToken(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenByJwtId(string id);
    Task<int> UpdateRefreshToken(RefreshToken refreshToken);
    Task<int> DeleteRefreshTokenByJwtId(string jwtId);
}

public class AuthRepository : IAuthRepository
{
    private readonly ILogger<IAuthRepository> _logger;
    private IDataContext _dataContext;
    private IDapperWrapper _dapperWrapper;
    public AuthRepository(ILogger<IAuthRepository> logger, IDataContext dataContext, IDapperWrapper dapperWrapper)
    {
        _logger = logger;
        _dataContext = dataContext;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<RefreshToken> GetRefreshTokenByJwtId(string jwtId)
    {
        _logger.LogDebug($"AuthRepository::GetRefreshTokenByJwtId jwtId: {jwtId}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                SELECT * FROM RefreshTokens
                WHERE JwtId = @jwtId
            ";
            return await _dapperWrapper.QueryFirstOrDefaultAsync<RefreshToken>(connection, sql, new { jwtId });
        }
    }

    public async Task<int> CreateRefreshToken(RefreshToken token)
    {
        _logger.Log(LogLevel.Debug, "AuthRepository::CreateRefreshToken");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                INSERT INTO RefreshTokens 
                (
                    UserGuid,
                    JwtId,
                    Token,
                    CreatedDate,
                    ExpirationDate
                )
                VALUES 
                (
                    @UserGuid,
                    @JwtId,
                    @Token,
                    @CreatedDate,
                    @ExpirationDate
                )
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, token);
        }
    }

    public async Task<int> UpdateRefreshToken(RefreshToken refreshToken)
    {
        _logger.LogDebug($"AuthRepository::UpdateRefreshToken id: {refreshToken.Id}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                UPDATE RefreshTokens 
                SET UserGuid = @UserGuid,
                    JwtId = @JwtId,
                    Token = @Token, 
                    ExpirationDate = @ExpirationDate, 
                    IsUsed = @IsUsed, 
                    IsValid = @IsValid
                WHERE Id = @Id
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, refreshToken);
        }
    }

    public async Task<int> DeleteRefreshTokenByJwtId(string jwtId)
    {
        _logger.LogDebug($"AuthRepository::DeleteRefreshTokenByJwtId jwtId: {jwtId}");
        using (var connection = _dataContext.CreateConnection())
        {
            var sql = @"
                DELETE FROM RefreshTokens
                WHERE JwtId = @jwtId
            ";
            return await _dapperWrapper.ExecuteAsync(connection, sql, new { jwtId });
        }
    }
}
