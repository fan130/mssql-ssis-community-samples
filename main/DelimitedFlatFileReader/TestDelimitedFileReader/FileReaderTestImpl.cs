using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;

namespace TestDelimitedFileReader
{
    internal class FileReaderTestImpl : IFileReader
    {
        bool randomCharacters = true;
        string sampleString = string.Empty;
        int count = 0;
        int currentIndex = 0;
        Random randomGenerator = new Random();

        bool isEof = false;

        internal FileReaderTestImpl(string sample)
        {
            this.sampleString = sample;
            this.randomCharacters = false;
        }

        internal FileReaderTestImpl(int count)
        {
            this.count = count;
        }

        internal FileReaderTestImpl(string sample, int count)
        {
            this.sampleString = sample;
            this.count = count;
            this.randomCharacters = false;
        }

        #region IFileReader Members

        public char GetNextChar()
        {
            char retChar = '\0';

            if (randomCharacters)
            {
                if (this.currentIndex < this.count)
                {
                    retChar = (char)randomGenerator.Next(char.MaxValue, char.MaxValue);
                    currentIndex++;
                }
                else
                {
                    this.isEof = true;
                }
            }
            else
            {
                if (this.count > 0)
                {
                    if (this.currentIndex < this.count)
                    {
                        retChar = this.sampleString[this.currentIndex % this.sampleString.Length];
                        currentIndex++;
                    }
                    else
                    {
                        this.isEof = true;
                    }
                }
                else if (this.currentIndex < this.sampleString.Length)
                {
                    retChar = this.sampleString[this.currentIndex];
                    currentIndex++;
                }
                else
                {
                    this.isEof = true;
                }
            }

            return retChar;
        }

        public bool IsEOF
        {
            get 
            {
                return this.isEof;
            }
        }

        #endregion
    }

}
