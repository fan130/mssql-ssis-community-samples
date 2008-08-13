using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader;

namespace TestDelimitedFileReader
{
    class RowParsingContextTestImpl : IRowParsingContext
    {

        #region IRowParsingContext Members

        public int ColumnCount
        {
            get { return 0; }
        }

        public void Append(char ch)
        {
        }

        #endregion
    }
}
