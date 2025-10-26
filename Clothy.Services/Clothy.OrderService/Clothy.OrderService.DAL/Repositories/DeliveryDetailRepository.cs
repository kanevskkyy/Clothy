using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Repositories
{
    public class DeliveryDetailRepository : GenericRepository<DeliveryDetail>, IDeliveryDetailRepository
    {
        public DeliveryDetailRepository(IConnectionFactory connectionFactory) : base(connectionFactory, "delivery_detail")
        {

        }
    }
}
