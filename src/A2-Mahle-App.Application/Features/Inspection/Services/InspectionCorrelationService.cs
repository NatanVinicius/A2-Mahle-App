using A2MahleApp.Application.Features.Inspection.Models;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2MahleApp.Application.Features.Inspection.Correlation;

public sealed class InspectionCorrelation
{
    private byte[]? _image;
    private InspectionResult? _result;

    public event EventHandler<InspectionEntity>? InspectionCompleted;

    public void ReceiveImage(byte[] image)
    {
        _image = image;
        TryComplete();
    }

    public void ReceiveResult(InspectionResult result)
    {
        _result = result;
        TryComplete();
    }

    private void TryComplete()
    {
        if (_image is null || _result is null)
        {
            return;
        }

        var inspection = new InspectionEntity
        {
            DateTime = DateTime.Now,
            Image = _image,
            Status = _result.Status,
            CycleTimeMilliseconds = _result.CycleTimeMilliseconds
        };

        _image = null;
        _result = null;

        InspectionCompleted?.Invoke(this, inspection);
    }
}
