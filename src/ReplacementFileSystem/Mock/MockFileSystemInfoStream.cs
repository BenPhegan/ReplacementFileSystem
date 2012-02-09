using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ReplacementFileSystem
{
    public class MockFileSystemInfoStream : MemoryStream
    {
        MockFileSystemInfo info;
        bool writable;

        public MockFileSystemInfoStream(MockFileSystemInfo info, bool writable)
            : base()
        {
            this.writable = writable;
            this.info = info;

            if (!this.writable)
            {
                this.writable = true;
                Write(info.FileData, 0, info.FileData.Length);
                Position = 0;
                this.writable = false;
            }
        }

        public override bool CanWrite
        {
            get { return writable; }
        }

        public override void Close()
        {
            Flush();

            if (writable)
                info.FileData = ToArray();

            base.Close();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!writable)
                throw new InvalidOperationException("the stream is not writable");

            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            if (!writable)
                throw new InvalidOperationException("the stream is not writable");

            base.WriteByte(value);
        }
    }

}
