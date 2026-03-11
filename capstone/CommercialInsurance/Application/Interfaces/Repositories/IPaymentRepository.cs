// Repository contract for payment records: retrieve payments by policy and check for duplicate transactions.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        // Returns all payment records associated with a given policy, ordered by payment date
        Task<IEnumerable<Payment>> GetPaymentsByPolicyIdAsync(string policyId);
    }
}
