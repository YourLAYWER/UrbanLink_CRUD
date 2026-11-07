using System.Collections.Concurrent;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;

namespace UrbanLink_CRUD.Models.Domain.SignalR
{
    public class TelemetryQueue
    {
        private readonly ConcurrentQueue<BusLocationUpdateDto> _locations = new();

        public void Enqueue(BusLocationUpdateDto location)
        {
            _locations.Enqueue(location);
        }

        public List<BusLocationUpdateDto> DequeueAll()
        {
            var batch = new List<BusLocationUpdateDto>();
            while (_locations.TryDequeue(out var location))
            {
                batch.Add(location);
            }
            return batch;
        }
    }
}
