using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace doru
{
    public class LinkMemoryStream : MemoryStream
    {
        public LinkMemoryStream() : base() { }
        public LinkMemoryStream(byte[] bts) : base(bts) { }
        public class CV
        {
            public int pos;
            public Stream _Stream;
        }
        public List<CV> cvs = new List<CV>();
        public void SetPointer(Stream _Stream, int pos)
        {
            cvs.Add(new CV { _Stream = _Stream, pos = pos });
        }

        Stream curstream;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (curstream != null)
            {
                int c = curstream.Read(buffer, offset, count);
                if (c == 0) curstream = null;
                else
                    return c;
            }
            foreach (CV cv in cvs)
            {
                if (Position < cv.pos && Position + count > cv.pos)
                {
                    cv._Stream.Seek(0, SeekOrigin.Begin);
                    curstream = cv._Stream;
                    return base.Read(buffer, 0, (int)(cv.pos - Position));
                }
            }
            return base.Read(buffer, offset, count);
        }
    }
}
