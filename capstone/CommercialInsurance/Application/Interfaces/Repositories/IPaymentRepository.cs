// Provides core functionality and structures for the application.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByPolicyIdAsync(string policyId);
    }
}
