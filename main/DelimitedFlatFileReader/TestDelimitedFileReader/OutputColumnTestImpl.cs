using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace TestDelimitedFileReader
{
    class OutputColumnTestImpl : IDTSOutputColumn100
    {
        int id = 0;
        string name = string.Empty;
        string description = string.Empty;

        DTSRowDisposition errorRowDisposition = DTSRowDisposition.RD_FailComponent;
        DTSRowDisposition truncationRowDisposition = DTSRowDisposition.RD_FailComponent;

        string errorOrTruncationOperation = string.Empty;

        CustomPropertyCollectionTestImpl customPropertyCollection = new CustomPropertyCollectionTestImpl();

        #region IDTSOutputColumn100 Members

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string IdentificationString
        {
            get { return string.Format("output \"{0}\" ({1})", this.Name, this.ID); }
        }

        public int CodePage
        {
            get { throw new NotImplementedException(); }
        }

        public int ComparisonFlags
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IDTSCustomPropertyCollection100 CustomPropertyCollection
        {
            get { return customPropertyCollection; }
        }

        public Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType DataType
        {
            get { throw new NotImplementedException(); }
        }

        public string ErrorOrTruncationOperation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DTSRowDisposition ErrorRowDisposition
        {
            get
            {
                return errorRowDisposition;
            }
            set
            {
                errorRowDisposition = value;
            }
        }

        public DTSRowDisposition TruncationRowDisposition
        {
            get
            {
                return truncationRowDisposition;
            }
            set
            {
                truncationRowDisposition = value;
            }
        }

        public int ExternalMetadataColumnID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Length
        {
            get { throw new NotImplementedException(); }
        }

        public int LineageID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int MappedColumnID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DTSObjectType ObjectType
        {
            get { return DTSObjectType.OT_OUTPUTCOLUMN; }
        }

        public int Precision
        {
            get { throw new NotImplementedException(); }
        }

        public int Scale
        {
            get { throw new NotImplementedException(); }
        }

        public void SetDataTypeProperties(Microsoft.SqlServer.Dts.Runtime.Wrapper.DataType eDataType, int lLength, int lPrecision, int lScale, int lCodePage)
        {
            throw new NotImplementedException();
        }

        public int SortKeyPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int SpecialFlags
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
