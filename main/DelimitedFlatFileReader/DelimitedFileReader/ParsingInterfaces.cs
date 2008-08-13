using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal enum ParsingResult
    {
        Match,
        Miss,
        Done
    }

    internal interface IParsingState
    {
        void AddNextState(IParsingState nextState);
        void SetErrorState(IParsingState errorState);

        ParsingResult ProcessCharacter(IParsingContext parsingContext, char nextChar);
    }

    internal interface IParsingContext
    {
        IParsingState CurrentState { get; set; }
        void Append(char ch);
        void Append(string text);
    }

    internal interface IRowParsingContext
    {
        int ColumnCount { get; }
        void Append(char ch);
    }

    public class FieldParsingException : Exception
    {
        public FieldParsingException(string message)
            : base(message)
        {
        }
    }

    public class ParsingBufferOverflowException : Exception
    {
        int columnIndex = 0;

        public int ColumnIndex
        {
            get { return columnIndex; }
            set { columnIndex = value; }
        }
    }

    public class RowColumnNumberOverflow : Exception
    {
    }
}
