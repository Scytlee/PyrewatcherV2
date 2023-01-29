using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class BroadcastersRepository : RepositoryBase, IBroadcastersRepository
{
  public BroadcastersRepository(IConfiguration config) : base(config)
  {
  }

  public async Task<IEnumerable<Channel>> GetConnected()
  {
    const string query = """
SELECT [b].*, [u].[DisplayName]
FROM [new].[Channels] AS [b]
INNER JOIN [Users] AS [u] ON [u].[Id] = [b].[Id]
WHERE [b].[Connected] = 1;
""";

    using var connection = await CreateConnection();

    var result = await connection.QueryAsync<Channel>(query);

    return result;
  }
}
