using System.Linq.Expressions;
using AutoMapper;
using TgBotGuide.Application.Dto;
using TgBotGuide.Application.Dto.Response;
using TgBotGuide.Application.Interfaces;
using TgBotGuide.Domain.Entities;
using TgBotGuide.Domain.Interfaces;

namespace TgBotGuide.Application.Services
{
    public class LocationService(
        ILocationRepository repository,
        IMapper mapper) : ILocationService
    {
        // Получение локации по ID
        public async Task<LocationResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var location = await repository.GetByIdAsync(id);
            return mapper.Map<LocationResponseDto>(location);
        }

        // Получение всех локаций
        public async Task<ICollection<LocationResponseDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            var locations = await repository.GetAllAsync();
            return mapper.Map<ICollection<LocationResponseDto>>(locations);
        }

        // Поиск локаций по условию
        public async Task<ICollection<LocationResponseDto>> FindAsync(Expression<Func<Location, bool>> predicate, CancellationToken cancellationToken)
        {
            var locations = await repository.FindAsync(predicate);
            return mapper.Map<ICollection<LocationResponseDto>>(locations);
        }

        // Добавление новой локации
        public async Task<LocationResponseDto> AddAsync(LocationDto dto, CancellationToken cancellationToken)
        {
            var location = mapper.Map<Location>(dto);
            await repository.AddAsync(location);
            return mapper.Map<LocationResponseDto>(location);
        }

        // Обновление локации
        public async Task<LocationResponseDto> UpdateAsync(Guid id, LocationDto dto, CancellationToken cancellationToken)
        {
            var location = mapper.Map<Location>(dto);
            location.Id = id;
            await repository.UpdateAsync(location);  // Используем асинхронный метод UpdateAsync
            return mapper.Map<LocationResponseDto>(location); // Возвращаем обновленную локацию
        }

        // Удаление локации
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var location = await repository.GetByIdAsync(id);
            if (location != null)
            {
                await repository.RemoveAsync(location);  // Используем асинхронный метод RemoveAsync
            }
        }
    }
}