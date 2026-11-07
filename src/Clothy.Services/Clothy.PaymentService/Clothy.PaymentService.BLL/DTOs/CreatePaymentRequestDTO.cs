using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.DTOs
{
    public class CreatePaymentRequestDTO
    {
        public Guid OrderId { get; set; }

        /// <summary>Optional basic device data for fraud scoring (shown to admins only).</summary>
        public PaymentDeviceDTO? Device { get; set; }
    }

    public class PaymentDeviceDTO
    {
        /// <summary>desktop or mobile.</summary>
        public string? DeviceType { get; set; }
        /// <summary>Device model or OS family, e.g. Windows, MacOS, iOS Device.</summary>
        public string? DeviceInfo { get; set; }
        /// <summary>Operating system, e.g. Windows 10, Android 13.</summary>
        public string? Os { get; set; }
        /// <summary>Browser, e.g. chrome 120.0.</summary>
        public string? Browser { get; set; }
        /// <summary>Screen resolution, e.g. 1920x1080.</summary>
        public string? Screen { get; set; }

        public FraudDeviceData ToFraudDeviceData() => new FraudDeviceData(DeviceType, DeviceInfo, Os, Browser, Screen);
    }
}
