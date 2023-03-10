using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HidSharp
{
    /// <summary>
    /// Communicates with a serial device.
    /// </summary>
    public abstract class SerialStream : DeviceStream
    {
        string _newLine;

        /// <exclude/>
        protected SerialStream(SerialDevice device)
            : base(device)
        {
            ReadTimeout = 3000;
            WriteTimeout = 3000;

            NewLine = "\r\n";
            BaudRate = SerialSettings.Default.BaudRate;
            DataBits = SerialSettings.Default.DataBits;
            Parity = SerialSettings.Default.Parity;
            StopBits = SerialSettings.Default.StopBits;
        }

        public string ReadTo(string ending)
        {
            Throw.If.NullOrEmpty(ending, "ending");

            var bytes = new List<byte>();
            var endingBytes = Encoding.GetBytes(ending);
            int matchBytes = 0;

            while (true)
            {
                int @byte = ReadByte();
                if (@byte < 0) { break; }
                bytes.Add((byte)@byte);

                if (@byte == endingBytes[matchBytes])
                {
                    if (++matchBytes == endingBytes.Length)
                    {
                        break;
                    }
                }
                else
                {
                    matchBytes = 0;
                }
            }

            @bytes.RemoveRange(bytes.Count - matchBytes, matchBytes);
            return Encoding.GetString(@bytes.ToArray());
        }

        [Obfuscation(Exclude = true)]
        public string ReadLine()
        {
            return ReadTo(NewLine);
        }

        [Obfuscation(Exclude = true)]
        public void Write(string s)
        {
            Throw.If.Null(s, "s");
            byte[] bytes = Encoding.GetBytes(s);
            Write(bytes, 0, bytes.Length);
        }

        [Obfuscation(Exclude = true)]
        public void WriteLine(string s)
        {
            Throw.If.Null(s, "s");
            Write(s + NewLine);
        }

        public abstract int BaudRate
        {
            get;
            set;
        }

        public abstract int DataBits
        {
            get;
            set;
        }

        public abstract SerialParity Parity
        {
            get;
            set;
        }

        public abstract int StopBits
        {
            get;
            set;
        }

        Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public string NewLine
        {
            get { return _newLine; }
            set
            {
                Throw.If.NullOrEmpty(value, "NewLine");
                _newLine = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="SerialDevice"/> associated with this stream.
        /// </summary>
        public new SerialDevice Device
        {
            get { return (SerialDevice)base.Device; }
        }
    }
}
