using System;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class BusMessage
    {
        public Guid CorrelationId { get; set; }
        public string Data { get; set; }
        public string RequestType { get; set; }
        public string ResponseType { get; set; }
    }
}
