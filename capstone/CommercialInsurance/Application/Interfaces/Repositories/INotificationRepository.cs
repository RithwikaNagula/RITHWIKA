// Repository contract for user notifications: get unread, mark read, and bulk insert.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
    }
}
