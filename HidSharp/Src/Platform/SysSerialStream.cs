using System;
using System.Threading;

namespace HidSharp.Platform
{
    abstract class SysSerialStream : SerialStream
    {
        protected SysSerialStream(SerialDevice device)
            : base(device)
        {

        }

        #region Reference Counting
        int _opened, _closed;
        int _refCount;

        internal void HandleInitAndOpen()
        {
            _opened = 1; _refCount = 1;
        }

        internal bool HandleClose()
        {
            return 0 == Interlocked.CompareExchange(ref _closed, 1, 0) && _opened != 0;
        }

        internal bool HandleAcquire()
        {
            while (true)
            {
                int refCount = _refCount;
                if (refCount == 0) { return false; }

                if (refCount == Interlocked.CompareExchange
                    (ref _refCount, refCount + 1, refCount))
                {
                    return true;
                }
            }
        }

        internal void HandleAcquireIfOpenOrFail()
        {
            if (_closed != 0 || !HandleAcquire()) { throw ExceptionForClosed(); }
        }

        internal void HandleRelease()
        {
            if (0 == Interlocked.Decrement(ref _refCount))
            {
                if (_opened != 0) { HandleFree(); }
            }
        }

        static Exception ExceptionForClosed()
        {
            return CommonException.CreateClosedException();
        }

        internal abstract void HandleFree();
        #endregion
    }
}
