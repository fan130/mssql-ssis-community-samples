using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace TestDelimitedFileReader
{
    class OutputTestImpl : IDTSOutput100
    {
        int id = 0;
        string name = string.Empty;
        string description = string.Empty;

        string errorOrTruncationOperation = string.Empty;

        DTSRowDisposition errorRowDisposition = DTSRowDisposition.RD_NotUsed;
        DTSRowDisposition truncationRowDisposition = DTSRowDisposition.RD_FailComponent;

        CustomPropertyCollectionTestImpl customPropertyCollection = new CustomPropertyCollectionTestImpl();
        OutputColumnCollectionTestImpl outputColumnCollection = new OutputColumnCollectionTestImpl();

        #region IDTSOutput100 Members

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

        public int Buffer
        {
            get { return 0; }
        }

        public IDTSComponentMetaData100 Component
        {
            get { return null; }
        }

        public IDTSCustomPropertyCollection100 CustomPropertyCollection
        {
            get { return customPropertyCollection; }
        }

        public bool Dangling
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool DeleteOutputOnPathDetached
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public string ErrorOrTruncationOperation
        {
            get { return errorOrTruncationOperation; }
            set { errorOrTruncationOperation = value; }
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

        public int ExclusionGroup
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public IDTSExternalMetadataColumnCollection100 ExternalMetadataColumnCollection
        {
            get { return null; }
        }

        public bool HasSideEffects
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool IsAttached
        {
            get { return false; }
        }

        public bool IsErrorOut
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

        public bool IsSorted
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public DTSObjectType ObjectType
        {
            get { return DTSObjectType.OT_OUTPUT; }
        }

        public IDTSOutputColumnCollection100 OutputColumnCollection
        {
            get { return this.outputColumnCollection; }
        }

        public int SynchronousInputID
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
