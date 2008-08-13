using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SqlServer.Dts.Pipeline.Wrapper;


namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class PropertiesManager
    {
        public event PostErrorDelegate PostErrorEvent = null;

        internal const string IsUnicodePropName = "IsUnicode";
        internal const string CodePagePropName = "CodePage";
        internal const string TextQualifierPropName = "TextQualifier";
        internal const string HeaderRowDelimiterPropName = "HeaderRowDelimiter";
        internal const string HeaderRowsToSkipPropName = "HeaderRowsToSkip";
        internal const string DataRowsToSkipPropName = "DataRowsToSkip";
        internal const string ColumnNamesInFirstRowPropName = "ColumnNamesInFirstRow";
        internal const string ColumnDelimiterPropName = "ColumnDelimiter";
        internal const string TreatEmptyStringsAsNullPropName = "TreatEmptyStringsAsNull";
        internal const string RowDelimiterPropName = "RowDelimiter";

        const string DefaultDelimiter = ",";
        const string DefaultRowDelimiter = "\r\n";
        const int DefaultCodePage = 1252;

        const int MaxDelimLength = 20;

        Dictionary<string, ValidateProperty> propertyValidationTable = new Dictionary<string, ValidateProperty>();

        public PropertiesManager()
        {
            this.SetPropertyValidationTable();
        }


        public static void AddComponentProperties(IDTSCustomPropertyCollection100 propertyCollection)
        {
            AddCustomProperty(propertyCollection, IsUnicodePropName, MessageStrings.IsUnicodePropDescription, true);
            AddCustomProperty(propertyCollection, TreatEmptyStringsAsNullPropName, MessageStrings.TreatEmptyStringsAsNullDescription, true);
            AddCustomProperty(propertyCollection, CodePagePropName, MessageStrings.CodePagePropDescription, DefaultCodePage);
            AddCustomProperty(propertyCollection, TextQualifierPropName, MessageStrings.TextQualifierPropDescription, string.Empty, typeof(DelimiterStringConverter).AssemblyQualifiedName);
            AddCustomProperty(propertyCollection, HeaderRowDelimiterPropName, MessageStrings.HeaderRowDelimiterPropDescription, DefaultRowDelimiter, typeof(DelimiterStringConverter).AssemblyQualifiedName);
            AddCustomProperty(propertyCollection, HeaderRowsToSkipPropName, MessageStrings.HeaderRowsToSkipPropDescription, 0);
            AddCustomProperty(propertyCollection, DataRowsToSkipPropName, MessageStrings.DataRowsToSkipPropDescription, 0);
            AddCustomProperty(propertyCollection, ColumnNamesInFirstRowPropName, MessageStrings.ColumnNameInFirstRowPropDescription, false);
            AddCustomProperty(propertyCollection, ColumnDelimiterPropName, MessageStrings.ColumnDelimiterPropDescription, DefaultDelimiter, typeof(DelimiterStringConverter).AssemblyQualifiedName);
            AddCustomProperty(propertyCollection, RowDelimiterPropName, MessageStrings.RowDelimiterPropDescription, DefaultRowDelimiter, typeof(DelimiterStringConverter).AssemblyQualifiedName);
        }

        private static void AddCustomProperty(IDTSCustomPropertyCollection100 propertyCollection, string name, string description, object defaultValue)
        {
            AddCustomProperty(propertyCollection, name, description, defaultValue, string.Empty);
        }

        private static void AddCustomProperty(IDTSCustomPropertyCollection100 propertyCollection, string name, string description, object value, string typeConverter)
        {
            IDTSCustomProperty100 property = propertyCollection.New();
            property.Name = name;
            property.Description = description;
            property.Value = value;
            property.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            if (value is string)
            {
                property.State = DTSPersistState.PS_PERSISTASHEX;
            }
            if (!string.IsNullOrEmpty(typeConverter))
            {
                property.TypeConverter = typeConverter;
            }
        }

        public static object GetPropertyValue(IDTSCustomPropertyCollection100 propertyCollection, string name)
        {
            for (int i = 0; i < propertyCollection.Count; i++)
            {
                IDTSCustomProperty100 property = propertyCollection[i];
                if (property.Name.Equals(name))
                {
                    return property.Value;
                }
            }

            return null;
        }

        public DTSValidationStatus ValidateProperties(IDTSCustomPropertyCollection100 customPropertyCollection, DTSValidationStatus oldStatus)
        {
            DTSValidationStatus resultStatus = oldStatus;
            foreach (IDTSCustomProperty100 property in customPropertyCollection)
            {
                resultStatus = ValidatePropertyValue(property.Name, property.Value, resultStatus);
            }

            return resultStatus;
        }

        public DTSValidationStatus ValidatePropertyValue(string propertyName, object propertyValue, DTSValidationStatus oldStatus)
        {
            DTSValidationStatus resultStatus = oldStatus;
            if (this.propertyValidationTable.ContainsKey(propertyName))
            {
                resultStatus = CommonUtils.CompareValidationValues(resultStatus, this.propertyValidationTable[propertyName](propertyName, propertyValue));
            }
            return resultStatus;
        }

        DTSValidationStatus ValidateBooleanProperty(string propertyName, object propertyValue)
        {
            if (propertyValue is bool)
            {
                return DTSValidationStatus.VS_ISVALID;
            }
            else
            {
                this.PostError(MessageStrings.InvalidPropertyValue(propertyName, propertyValue));
                return DTSValidationStatus.VS_ISCORRUPT;
            }
        }

        DTSValidationStatus ValidateDelimiterProperty(string propertyName, object propertyValue)
        {
            if (propertyValue is string)
            {
                string value = (string)propertyValue;
                if (value.Length < MaxDelimLength)
                {
                    return DTSValidationStatus.VS_ISVALID;
                }
                else
                {
                    this.PostError(MessageStrings.PropertyStringTooLong(propertyName, propertyValue.ToString()));
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }
            else
            {
                this.PostError(MessageStrings.InvalidPropertyValue(propertyName, propertyValue));
                return DTSValidationStatus.VS_ISCORRUPT;
            }
        }

        DTSValidationStatus ValidateRowDelimiterProperty(string propertyName, object propertyValue)
        {
            DTSValidationStatus retStatus = ValidateDelimiterProperty(propertyName, propertyValue);
            if (retStatus == DTSValidationStatus.VS_ISVALID)
            {
                string value = (string)propertyValue;
                if (value.Length == 0)
                {
                    this.PostError(MessageStrings.RowDelimiterEmpty);
                    retStatus = DTSValidationStatus.VS_ISBROKEN;
                }
            }

            return retStatus;
        }


        private void SetPropertyValidationTable()
        {
            this.propertyValidationTable.Add(IsUnicodePropName, new ValidateProperty(ValidateBooleanProperty));
            this.propertyValidationTable.Add(ColumnNamesInFirstRowPropName, new ValidateProperty(ValidateBooleanProperty));
            this.propertyValidationTable.Add(TreatEmptyStringsAsNullPropName, new ValidateProperty(ValidateBooleanProperty));

            this.propertyValidationTable.Add(HeaderRowDelimiterPropName, new ValidateProperty(ValidateDelimiterProperty));
            this.propertyValidationTable.Add(ColumnDelimiterPropName, new ValidateProperty(ValidateDelimiterProperty));
            this.propertyValidationTable.Add(RowDelimiterPropName, new ValidateProperty(ValidateRowDelimiterProperty));
            this.propertyValidationTable.Add(TextQualifierPropName, new ValidateProperty(ValidateDelimiterProperty));
        }

        private void PostError(string errorMessage)
        {
            if (this.PostErrorEvent != null)
            {
                this.PostErrorEvent(errorMessage);
            }
        }

        delegate DTSValidationStatus ValidateProperty(string propertyName, object propertyValue);
    }

    internal delegate void PostErrorDelegate(string errorMessage);

}
