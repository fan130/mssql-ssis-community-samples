using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    internal class CommonUtils
    {
        public static DTSValidationStatus CompareValidationValues(DTSValidationStatus oldStatus, DTSValidationStatus newStatus)
        {
            if (oldStatus == DTSValidationStatus.VS_ISVALID && newStatus == DTSValidationStatus.VS_ISVALID)
            {
                return DTSValidationStatus.VS_ISVALID;
            }
            if (oldStatus == DTSValidationStatus.VS_ISCORRUPT || newStatus == DTSValidationStatus.VS_ISCORRUPT)
            {
                return DTSValidationStatus.VS_ISCORRUPT;
            }
            else
            {
                return DTSValidationStatus.VS_ISBROKEN;
            }
        }

        public static void FilterOutFixedWidthConnections(ArrayList connList)
        {
            for (int i = connList.Count - 1; i >= 0; i--)
            {
                ConnectionManager connManager = connList[i] as ConnectionManager;
                if (string.Compare("Delimited", (string)connManager.Properties["Format"].GetValue(connManager)) != 0)
                {
                    connList.RemoveAt(i);
                }
            }
        }

        public static string GetNewConnectionName(Connections connections, string suggestedName)
        {
            string retValue = string.Empty;

            if (!string.IsNullOrEmpty(suggestedName))
            {
                // check to see if suggestedname is already taken 
                string newSuggestedName = suggestedName;

                int currentIndex = 1;

                while (connections.Contains(newSuggestedName))
                {
                    newSuggestedName = string.Format("{0} {1}", suggestedName, currentIndex);
                    currentIndex++;
                }

                retValue = newSuggestedName;
            }

            return retValue;
        }

    }
}
