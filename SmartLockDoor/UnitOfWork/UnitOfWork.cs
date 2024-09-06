using MySqlConnector;
using System.Data.Common;

namespace SmartLockDoor
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbConnection? _connection = null;

        private readonly string _connectionString;

        public UnitOfWork(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString"] ?? "";
        }

        public DbConnection Connection => _connection ??= new MySqlConnection(_connectionString);
    }
}
