using System;
using System.Collections.Generic;
using System.Linq;

namespace HidSharp.Platform.MacOS
{
    sealed class MacHidManager : HidManager
    {
        protected override SystemEvents.EventManager CreateEventManager()
        {
            return new SystemEvents.MacOSEventManager();
        }

        protected override void Run(Action readyCallback)
        {
            using (var manager = NativeMethods.IOHIDManagerCreate(IntPtr.Zero).ToCFType())
            {
                RunAssert(manager.IsSet, "HidSharp IOHIDManagerCreate failed.");

                using (var matching = NativeMethods.IOServiceMatching("IOHIDDevice").ToCFType())
                {
                    RunAssert(matching.IsSet, "HidSharp IOServiceMatching failed.");

                    var devicesChangedCallback = new NativeMethods.IOHIDDeviceCallback(DevicesChangedCallback);
                    NativeMethods.IOHIDManagerSetDeviceMatching(manager.Handle, matching.Handle);
                    NativeMethods.IOHIDManagerRegisterDeviceMatchingCallback(manager.Handle, devicesChangedCallback, IntPtr.Zero);
                    NativeMethods.IOHIDManagerRegisterDeviceRemovalCallback(manager.Handle, devicesChangedCallback, IntPtr.Zero);

                    var runLoop = NativeMethods.CFRunLoopGetCurrent();
                    NativeMethods.CFRetain(runLoop);
                    NativeMethods.IOHIDManagerScheduleWithRunLoop(manager, runLoop, NativeMethods.kCFRunLoopDefaultMode);
                    try
                    {
                        readyCallback();
                        NativeMethods.CFRunLoopRun();
                    }
                    finally
                    {
                        NativeMethods.IOHIDManagerUnscheduleFromRunLoop(manager, runLoop, NativeMethods.kCFRunLoopDefaultMode);
                        NativeMethods.CFRelease(runLoop);
                    }

                    GC.KeepAlive(devicesChangedCallback);
                }
            }
        }

        static void DevicesChangedCallback(IntPtr context, NativeMethods.IOReturn result, IntPtr sender, IntPtr device)
        {
            DeviceList.Local.RaiseChanged();
        }

        object[] GetDeviceKeys(string kind)
        {
            var paths = new List<NativeMethods.io_string_t>();

            var matching = NativeMethods.IOServiceMatching(kind).ToCFType(); // Consumed by IOServiceGetMatchingServices, so DON'T Dispose().
            if (matching.IsSet)
            {
                if (NativeMethods.IOReturn.Success == NativeMethods.IOServiceGetMatchingServices(0, matching, out int iteratorObj))
                {
                    using (var iterator = iteratorObj.ToIOObject())
                    {
                        while (true)
                        {
                            using (var handle = NativeMethods.IOIteratorNext(iterator).ToIOObject())
                            {
                                if (!handle.IsSet) { break; }

                                if (NativeMethods.IOReturn.Success == NativeMethods.IORegistryEntryGetPath(handle, "IOService", out NativeMethods.io_string_t path))
                                {
                                    paths.Add(path);
                                }
                            }
                        }
                    }
                }
            }

            return paths.Cast<object>().ToArray();
        }

        protected override object[] GetBleDeviceKeys()
        {
            return new object[0];
        }

        protected override object[] GetHidDeviceKeys()
        {
            return GetDeviceKeys("IOHIDDevice");
        }

        protected override object[] GetSerialDeviceKeys()
        {
            return GetDeviceKeys("IOSerialBSDClient");
        }

        protected override bool TryCreateBleDevice(object key, out Device device)
        {
            throw new NotImplementedException();
        }

        protected override bool TryCreateHidDevice(object key, out Device device)
        {
            device = MacHidDevice.TryCreate((NativeMethods.io_string_t)key);
            return device != null;
        }

        protected override bool TryCreateSerialDevice(object key, out Device device)
        {
            device = MacSerialDevice.TryCreate((NativeMethods.io_string_t)key);
            return device != null;
        }

        public override string FriendlyName
        {
            get { return "Mac OS HID"; }
        }

        public override bool IsSupported
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    try
                    {
                        NativeMethods.OSErr majorErr = NativeMethods.Gestalt(NativeMethods.OSType.gestaltSystemVersionMajor, out IntPtr major);
                        NativeMethods.OSErr minorErr = NativeMethods.Gestalt(NativeMethods.OSType.gestaltSystemVersionMinor, out IntPtr minor);
                        if (majorErr == NativeMethods.OSErr.noErr && minorErr == NativeMethods.OSErr.noErr)
                        {
                            return (long)major >= 10 || ((long)major == 10 && (long)minor >= 6);
                        }
                    }
                    catch
                    {

                    }
                }

                return false;
            }
        }
    }
}
