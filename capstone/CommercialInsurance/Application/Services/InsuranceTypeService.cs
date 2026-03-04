// Provides core functionality and structures for the application.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class InsuranceTypeService : IInsuranceTypeService
    {
        private readonly IGenericRepository<InsuranceType> _repository;

        public InsuranceTypeService(IGenericRepository<InsuranceType> repository)
        {
            _repository = repository;
        }

        public async Task<InsuranceTypeDto> CreateInsuranceTypeAsync(CreateInsuranceTypeDto dto)
        {
            var id = await GenerateNextId();
            var insuranceType = new InsuranceType
            {
                Id = id,
                TypeName = dto.TypeName,
                Description = dto.Description,
                IsActive = true
            };

            await _repository.AddAsync(insuranceType);

            return new InsuranceTypeDto
            {
                Id = insuranceType.Id,
                TypeName = insuranceType.TypeName,
                Description = insuranceType.Description,
                IsActive = insuranceType.IsActive,
                CreatedAt = insuranceType.CreatedAt
            };
        }

        public async Task<IEnumerable<InsuranceTypeDto>> GetAllInsuranceTypesAsync()
        {
            var types = await _repository.GetAllAsync();
            return types.Select(t => new InsuranceTypeDto
            {
                Id = t.Id,
                TypeName = t.TypeName,
                Description = t.Description,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            });
        }

        public async Task<InsuranceTypeDto?> GetInsuranceTypeByIdAsync(string id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return null;

            return new InsuranceTypeDto
            {
                Id = type.Id,
                TypeName = type.TypeName,
                Description = type.Description,
                IsActive = type.IsActive,
                CreatedAt = type.CreatedAt
            };
        }

        public async Task<bool> DeleteInsuranceTypeAsync(string id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return false;

            await _repository.DeleteAsync(type);
            return true;
        }

        private async Task<string> GenerateNextId()
        {
            var all = await _repository.GetAllAsync();
            var count = all.Count();
            return $"typ{count + 1}";
        }
    }
}
