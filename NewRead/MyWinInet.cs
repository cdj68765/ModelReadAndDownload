using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;
using WinInetDemo3;

namespace Nico3D模型获取工具
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal class InetHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal InetHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            bool flag = NativeMethods.InternetCloseHandle(this.handle);
            if (!flag)
            {
                Marshal.GetLastWin32Error();
            }
            return flag;
        }
    }

    internal class MyBytesReadEventArgs : EventArgs
    {
        private int _bytesRead;

        private int _totalBytes;

        internal int BytesRead
        {
            get
            {
                return this._bytesRead;
            }
        }

        internal int TotalBytes
        {
            get
            {
                return this._totalBytes;
            }
        }

        internal MyBytesReadEventArgs(int bytesRead, int totalBytes)
        {
            this._bytesRead = bytesRead;
            this._totalBytes = totalBytes;
        }
    }

    internal class DownloadHelper : IDisposable
    {
        private class MyInternetReadStream : Stream
        {
            internal delegate void BytesReadCallback(int bytesRead, int totalBytes);

            private const int _localBufferSize = 32768;

            private readonly IntPtr _hInetFile;

            private byte[] _localBuffer;

            private readonly int _contentLength;

            private BytesReadCallback _bytesReadCallback;

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException();
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            internal MyInternetReadStream(IntPtr hInetFile, DownloadHelper.MyInternetReadStream.BytesReadCallback bytesReadCallback)
            {
                this._hInetFile = hInetFile;
                this._localBuffer = new byte[BufferSize];
                this._bytesReadCallback = bytesReadCallback;
                this._contentLength = DownloadHelper.GetContentLength(hInetFile);
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (buffer.Length < offset + count)
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                int dwNumberOfBytesToRead = Math.Min(BufferSize, count);
                int length = 0;
                if (!NativeMethods.InternetReadFile(this._hInetFile, this._localBuffer, dwNumberOfBytesToRead, out length))
                {
                    // todo
                }
                Array.Copy(this._localBuffer, 0, buffer, offset, length);
                this._bytesReadCallback(length, this._contentLength);
                return length;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this._bytesReadCallback = null;
                    this._localBuffer = null;
                }
                base.Dispose(disposing);
            }
        }

        private const int BufferSize = 32768;
        private EventHandler<MyBytesReadEventArgs> _bytesRead;
        private InetHandle _hInet;
        private bool _cancelDownload;
        private bool _disposed;

        internal event EventHandler<MyBytesReadEventArgs> BytesRead
        {
            add
            {
                this._bytesRead = (EventHandler<MyBytesReadEventArgs>)Delegate.Combine(this._bytesRead, value);
            }
            remove
            {
                this._bytesRead = (EventHandler<MyBytesReadEventArgs>)Delegate.Remove(this._bytesRead, value);
            }
        }


        internal static string Referer
        {
            get
            {
                return "test referer";
            }
        }


        internal void InternetClose()
        {
            this._hInet.Dispose();
            this._hInet.SetHandleAsInvalid();
        }
        public string DownloadStatus="";
        public long End = 0;
        public int current = 0;
        internal bool DownloadFile(string uri, out byte[] stream)
        {
            var urI = new Uri(uri);
            stream = new byte[1024];
            //string contentDispositionFileName = null;
            DownloadStatus = "建立连接";
            _hInet = NativeMethods.InternetOpen("", 0, null, null, 0);
            if (_hInet.IsInvalid)
            {
                throw new InvalidOperationException("InternetOpen has not been called yet");
            }
            IntPtr response = IntPtr.Zero;
            var isDownloadSuccess = false;
            try
            {
                DownloadStatus = "验证回复状态";
                isDownloadSuccess = CheckResponseStatus(ref urI, ref response);
                if (isDownloadSuccess)
                {
                    End  = GetContentLength(response);

                    using (Stream internetStream = GetInternetStream(response))
                    {
                        DownloadStatus = "正在下载模型数据中";
                        stream = new byte[End];
                        for (int i = 0; i < stream.Length; i++)
                        {
                            current = i;
                            stream[i] =(byte) internetStream.ReadByte();
                        }
                    }
                }
            }
            finally
            {
                if (response != IntPtr.Zero)
                {
                    NativeMethods.InternetCloseHandle(response);
                    response = IntPtr.Zero;
                }
            }
            DownloadStatus = "下载完成";
            return isDownloadSuccess;
        }

        private Stream GetInternetStream(IntPtr hInetFile)
        {
            string contentEncoding = GetContentEncoding(hInetFile);
            if (contentEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return new GZipStream(this.ForGZipReadStream(hInetFile), CompressionMode.Decompress, false);
            }
            return new MyInternetReadStream(hInetFile, new MyInternetReadStream.BytesReadCallback(this.BytesReadCallback));
        }

        private Stream ForGZipReadStream(IntPtr hInetFile)
        {
            return new MyInternetReadStream(hInetFile, new MyInternetReadStream.BytesReadCallback(this.BytesReadCallback));
        }

        private bool CheckResponseStatus(ref Uri uri, ref IntPtr hInetFile)
        {
            byte[] content = new byte[BufferSize];
            string referer = "Referer: " + DownloadHelper.Referer;
            referer += "\nAccept-Encoding: gzip";
            // INTERNET_FLAG_RELOAD
            // 0x80000000  -> 2147483648
            // Forces a download of the requested file, from the origin server, not from the cache. 
            hInetFile = NativeMethods.InternetOpenUrl(this._hInet, uri.AbsoluteUri, referer, referer.Length, 2149580800, IntPtr.Zero);
            if (hInetFile == IntPtr.Zero)
            {
                // todo
            }
            int count = BufferSize;
            int temp = 0;
            if (!NativeMethods.HttpQueryInfo(hInetFile, 19, content, out count, out temp))
            {
                // todo
            }
            return string.Equals(Encoding.Unicode.GetString(content, 0, count), "200", StringComparison.Ordinal);
        }

        private static string GetContentEncoding(IntPtr hInetFile)
        {
            int length1 = 0;
            int length2 = 100;
            byte[] content = new byte[length2];
            string text = string.Empty;
            try
            {
                if (NativeMethods.HttpQueryInfo(hInetFile, 29, content, out length2, out length1))
                {
                    text = Encoding.Unicode.GetString(content, 0, length2);
                }
            }
            catch
            {
            }
            return text;
        }

        private static int GetContentLength(IntPtr hInetFile)
        {
            int result = 1;
            try
            {
                int num = 100;
                byte[] array = new byte[num];
                int num2 = 0;
                if (NativeMethods.HttpQueryInfo(hInetFile, 5u, array, out num, out num2))
                {
                    string tempStr = Encoding.Unicode.GetString(array, 0, num);
                    result = int.Parse(tempStr, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
            }
            return result;
        }


        private void BytesReadCallback(int bytesReadLength, int totalBytesLength)
        {
            EventHandler<MyBytesReadEventArgs> tempHandler = this._bytesRead;
            if (tempHandler != null)
            {
                tempHandler(this, new MyBytesReadEventArgs(bytesReadLength, totalBytesLength));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (this._hInet != null && !this._hInet.IsInvalid)
                {
                    this._hInet.Dispose();
                }
                this._disposed = true;
            }
        }
    }
}

