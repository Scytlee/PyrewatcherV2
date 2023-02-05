using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class ChannelsRepository : IChannelsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;

  public ChannelsRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
  }

  public async Task<IEnumerable<Channel>> GetConnected()
  {
    const string query = """
SELECT [c].*, [u].[DisplayName], [u].[NormalizedName]
FROM [Core].[Channels] AS [c]
INNER JOIN [Core].[Users] AS [u] ON [u].[Id] = [c].[Id]
WHERE [c].[Connected] = 1;
""";

    using var connection = await _connectionFactory.CreateConnection();

    try
    {
      var result = await _dapperWrapper.QueryAsync<Channel>(connection, query);
      return result;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
