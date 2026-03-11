// Repository contract for insurance type CRUD, including eager loading of associated plans.
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IInsuranceTypeRepository : IGenericRepository<InsuranceType>
    {
    }
}
