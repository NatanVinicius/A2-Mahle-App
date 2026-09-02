using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2MahleApp.Application.Features.Inspection.Services;

public interface IInspectionRepository
{
    Task AddAsync(InspectionEntity inspection, CancellationToken cancellationToken = default);
}
