// Core CRUD operations for insurance categories (e.g., Cyber, Auto, Liability), including duplication checks.
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class InsuranceTypeService : IInsuranceTypeService
    {
        private readonly IInsuranceTypeRepository _repository;

        // Abstracts direct database interactions behind the generic repository interface
        public InsuranceTypeService(IInsuranceTypeRepository repository)
        {
            _repository = repository;
        }

        // Creates a new insurance type (e.g. "Workers Compensation").
        // Validates that an identically named type doesn't already exist to prevent duplicate categories in the UI.
        public async Task<InsuranceTypeDto> CreateInsuranceTypeAsync(CreateInsuranceTypeDto dto)
        {
            var existingTypes = await _repository.GetAllAsync();

            // Name collision guard — throws if standardizing to ordinal casing reveals a match
            if (existingTypes.Any(t => t.TypeName.Equals(dto.TypeName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("An insurance type with this name already exists.");
            }

            var type = new InsuranceType
            {
                Id = await GenerateNextId(),
                TypeName = dto.TypeName,
                Description = dto.Description
            };

            await _repository.AddAsync(type);
            return MapToDto(type);
        }

        // Retrieves all categories, typically to populate frontend dropdowns for quoting or plan exploration.
        public async Task<IEnumerable<InsuranceTypeDto>> GetAllInsuranceTypesAsync()
        {
            var types = await _repository.GetAllAsync();
            return types.Select(MapToDto);
        }

        // Fetch a single category by primary key.
        public async Task<InsuranceTypeDto?> GetInsuranceTypeByIdAsync(string id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return null;
            return MapToDto(type);
        }

        // Modifies an existing category's Name or Description, without touching its associated Plans.
        public async Task<InsuranceTypeDto?> UpdateInsuranceTypeAsync(string id, CreateInsuranceTypeDto dto)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return null;

            type.TypeName = dto.TypeName;
            type.Description = dto.Description;

            await _repository.UpdateAsync(type);
            return MapToDto(type);
        }

        // Deletes the category permanently; if plans are attached, the DB FK constraints will prevent this 
        // unless Cascade Delete is configured on the EntityFramework side.
        public async Task<bool> DeleteInsuranceTypeAsync(string id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return false;

            await _repository.DeleteAsync(type);
            return true;
        }

        // Generates a sequential string ID (e.g. itype1, itype2) to maintain human-readable categorizations.
        private async Task<string> GenerateNextId()
        {
            var all = await _repository.GetAllAsync();
            var count = all.Count();
            return $"itype{count + 1}";
        }

        // Transformer logic translating the backend domain model to the frontend-safe Data Transfer Object.
        private InsuranceTypeDto MapToDto(InsuranceType type)
        {
            return new InsuranceTypeDto
            {
                Id = type.Id,
                TypeName = type.TypeName,
                Description = type.Description
            };
        }
    }
}
