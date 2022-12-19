using HidSharp.Reports;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace HidSharp
{
    /// <summary>
    /// Represents a USB HID class device.
    /// </summary>
    [ComVisible(true), Guid("4D8A9A1A-D5CC-414e-8356-5A025EDA098D")]
    public abstract class HidDevice : Device
    {
        /// <inheritdoc/>
        public new HidStream Open()
        {
            return (HidStream)base.Open();
        }

        /// <inheritdoc/>
        public new HidStream Open(OpenConfiguration openConfig)
        {
            return (HidStream)base.Open(openConfig);
        }

        /// <inheritdoc/>
        public override string GetFriendlyName()
        {
            return GetProductName();
        }

        /// <summary>
        /// Returns the manufacturer name.
        /// </summary>
        public abstract string GetManufacturer();

        /// <summary>
        /// Returns the product name.
        /// </summary>
        public abstract string GetProductName();

        /// <summary>
        /// Returns the device serial number.
        /// </summary>
        public abstract string GetSerialNumber();

        /// <summary>
        /// Returns the maximum input report length, including the Report ID byte.
        /// If the device does not use Report IDs, the first byte will always be 0.
        /// </summary>
        public abstract int GetMaxInputReportLength();

        /// <summary>
        /// Returns the maximum output report length, including the Report ID byte.
        /// If the device does not use Report IDs, use 0 for the first byte.
        /// </summary>
        public abstract int GetMaxOutputReportLength();

        /// <summary>
        /// Returns the maximum feature report length, including the Report ID byte.
        /// If the device does not use Report IDs, use 0 for the first byte.
        /// </summary>
        public abstract int GetMaxFeatureReportLength();

        /// <summary>
        /// Retrieves and parses the report descriptor of the USB device.
        /// </summary>
        /// <returns>The parsed report descriptor.</returns>
        public ReportDescriptor GetReportDescriptor()
        {
            return new ReportDescriptor(GetRawReportDescriptor());
        }

        /// <summary>
        /// Returns the raw report descriptor of the USB device.
        /// </summary>
        /// <returns>The raw report descriptor.</returns>
        public virtual byte[] GetRawReportDescriptor()
        {
            throw new NotSupportedException(); // Windows reconstructs it. Linux can retrieve it. MacOS 10.8+ can retrieve it as well.
        }

        public uint GetTopLevelUsage()
        {
            var reportDescriptor = GetReportDescriptor();
            var ditem = reportDescriptor.DeviceItems.FirstOrDefault();
            return ditem.Usages.GetAllValues().FirstOrDefault();
        }

        /*
        TODO
        public virtual string[] GetDevicePathHierarchy()
        {
            throw new NotSupportedException();
        }
        */

        /// <summary>
        /// Returns the serial ports of the composite USB device.
        /// Currently this is only supported on Windows.
        /// </summary>
        /// <returns>Serial ports of the USB device.</returns>
        public virtual string[] GetSerialPorts()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string manufacturer = "(unnamed manufacturer)";
            try { manufacturer = GetManufacturer(); } catch { }

            string productName = "(unnamed product)";
            try { productName = GetProductName(); } catch { }

            string serialNumber = "(no serial number)";
            try { serialNumber = GetSerialNumber(); } catch { }

            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} (VID {3}, PID {4}, version {5})",
                manufacturer, productName, serialNumber, VendorID, ProductID, ReleaseNumber);
        }

        /// <inheritdoc/>
        public bool TryOpen(out HidStream stream)
        {
            return TryOpen(null, out stream);
        }

        /// <inheritdoc/>
        public bool TryOpen(OpenConfiguration openConfig, out HidStream stream)
        {
            bool result = base.TryOpen(openConfig, out DeviceStream baseStream);
            stream = (HidStream)baseStream; return result;
        }

        public override bool HasImplementationDetail(Guid detail)
        {
            return base.HasImplementationDetail(detail) || detail == ImplementationDetail.HidDevice;
        }

        /// <summary>
        /// The USB product ID. These are listed at: http://usb-ids.gowdy.us
        /// </summary>
        public abstract int ProductID
        {
            get;
        }

        /// <summary>
        /// The device release number.
        /// </summary>
        public Version ReleaseNumber
        {
            get { return Utility.BcdHelper.ToVersion(ReleaseNumberBcd); }
        }

        /// <summary>
        /// The device release number, in binary-coded decimal.
        /// </summary>
        public abstract int ReleaseNumberBcd
        {
            get;
        }

        /// <summary>
        /// The USB vendor ID. These are listed at: http://usb-ids.gowdy.us
        /// </summary>
        public abstract int VendorID
        {
            get;
        }
    }
}
