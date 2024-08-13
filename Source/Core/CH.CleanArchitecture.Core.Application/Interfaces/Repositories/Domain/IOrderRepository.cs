using System;
using System.Threading.Tasks;
using CH.CleanArchitecture.Core.Domain.Order;
using CH.Data.Abstractions;

namespace CH.CleanArchitecture.Core.Application
{
    public interface IOrderRepository : IAggregateRepository<Order, Guid>
    {
        Task SaveToEventStoreAsync(Order order);
    }
}