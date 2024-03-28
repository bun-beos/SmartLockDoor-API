using System.Data.Common;

namespace SmartLockDoor
{
    public interface IUnitOfWork
    {
        DbConnection Connection { get; }

    }
}
