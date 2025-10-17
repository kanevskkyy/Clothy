using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs
{
    public class DeliveryDetailCreateDTO
    {
        public Guid ProviderId { get; set; }
        public Guid CityId { get; set; }
        public string? PostalIndex { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? DetailsDescription { get; set; }
    }
}
