using System.Data;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IDbConnectionFactory
{
  Task<IDbConnection> CreateConnection();
}
