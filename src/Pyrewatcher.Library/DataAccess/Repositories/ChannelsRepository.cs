using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class ChannelsRepository : RepositoryBase, IChannelsRepository
{
  public ChannelsRepository(IConfiguration configuration) : base(configuration)
  {
  }

  public async Task<IEnumerable<Channel>> GetConnected()
  {
    const string query = """
SELECT [c].*, [u].[DisplayName], [u].[NormalizedName]
FROM [Core].[Channels] AS [c]
INNER JOIN [Core].[Users] AS [u] ON [u].[Id] = [c].[Id]
WHERE [c].[Connected] = 1;
""";

    using var connection = await CreateConnection();

    try
    {
      var result = await connection.QueryAsync<Channel>(query);
      return result;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
