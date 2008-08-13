using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal interface IFileReader
    {
        char GetNextChar();

        bool IsEOF { get; }
    }

    internal class FileReader : IFileReader
    {
        const int EOF = -1;
        const int BufferSize = 65536;

        bool endOfFile = false;
        FileStream stream = null;
        BinaryReader reader = null;

        public FileReader(string fileName, Encoding encoding)
        {
            ArgumentVerifier.CheckObjectArgument(encoding, "encoding");

            this.OpenStream(fileName);
            reader = new BinaryReader(stream, encoding);

            this.SkipBOM();
        }

        public void Close()
        {
            this.reader.Close();
            this.stream.Close();
        }

        private void OpenStream(string fileName)
        {
            ArgumentVerifier.CheckFileNameArgument(fileName);

            stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);
        }

        #region IFileReader Members

        public char GetNextChar()
        {
            int retChar =  this.reader.Read();
            if (retChar == EOF)
            {
                endOfFile = true;
            }

            return (char)retChar;
        }

        public bool IsEOF
        {
            get 
            {
                return endOfFile;
            }
        }

        private void SkipBOM()
        {
            byte[] byteBuffer = reader.ReadBytes(4);

            int count = CountBOMBytes(byteBuffer);
            // Move the file position if less than 4 bytes are used.
            if (count != 4)
            {
                reader.BaseStream.Seek(count, SeekOrigin.Begin);
            }
        }

        internal static int CountBOMBytes(byte[] byteBuffer)
        {
            if (byteBuffer[0] == 0xFE && byteBuffer[1] == 0xFF)
            {
                // Big Endian Unicode
                return 2;
            }
            else if (byteBuffer[0] == 0xFF && byteBuffer[1] == 0xFE)
            {
                // Little Endian Unicode, or possibly little endian UTF32
                return 2;
            }
            else if (byteBuffer[0] == 0xEF && byteBuffer[1] == 0xBB && byteBuffer[2] == 0xBF)
            {
                // UTF-8
                return 3;
            }
            else if (byteBuffer[0] == 0 && byteBuffer[1] == 0 && byteBuffer[2] == 0xFE && byteBuffer[3] == 0xFF)
            {
                // Big Endian UTF32
                return 4;
            }
            else
            {
                // Unknown - rewind back to the start.
                return 0;
            }
        }

        #endregion
    }
}
