using Moq;
using System.Data;
using friasco_api.Data;

namespace friasco_api_test.Data;

[TestFixture]
public class DataContextTests
{
    private Mock<IDbConnection> _connectionMock;
    private IDataContext _dataContext;

    [SetUp]
    public void SetUp()
    {
        _connectionMock = new Mock<IDbConnection>();
        _dataContext = new DataContext(() => _connectionMock.Object);
    }
}
