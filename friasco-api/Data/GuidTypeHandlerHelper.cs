using System.Data;
using Dapper;

namespace friasco_api.Helpers;

public class GuidTypeHandlerHelper : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return new Guid((string)value);
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}
