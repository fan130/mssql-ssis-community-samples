using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.WmiSourceAdapter
{
    // This helper class is used to unwind WMI row ( unwind array in any column )
    // we use the same approach to handle WMI arrays as WMI ODBC driver 
    // input row is unwinded into several output rows
    // output rows should represent every possible combination of WMI arrays elements in different columns
    // for details see msdn :
    // http://msdn.microsoft.com/en-us/library/aa392328(VS.85).aspx#_hmm_mapping_wmi_arrays_to_odbc
    // 
    // Here is an example :
    //
    // input row : { a1, {b1,b2}, {c1,c2,c3} }
    //
    // output rows :
    // 
    // { a1 , b1, c1 }
    // { a1 , b1, c2 }
    // { a1 , b1, c3 }
    // { a1 , b2, c1 }
    // { a1 , b2, c2 }
    // { a1 , b2, c3 }

    class RowUnwinder
    {


        /// <summary>
        /// Unwind given array into several output arrays by unwinding sub-arrays
        /// </summary>
        /// <param name="arr">input array (may contain sub-arrays)</param>      
        /// <returns>A list of arrays( each array does NOT contain sub-arrays ) representing every 
        /// possible combination of input array sub-arrays elements in different positions
        /// </returns>        
        public static List<object[]> UnwindRow(object[] arr)
        {
            object[] buffer = new object[arr.Length];
            List<object[]> listResult = new List<object[]>();

            UnwindRow(arr, 0, ref buffer, ref listResult);
            return listResult;
        }

        public static void UnwindRow(object[] arr, int i, ref object[] buffer, ref List<object[]> listResult)
        {
            for (; i < arr.Length; buffer[i] = arr[i], ++i)
            {
                // if column type is not array - just iterate to next column
                if( arr[i] == null || !arr[i].GetType().IsArray ) 
                    continue;

                // if we deal with subarray - lets unwind it
                Array inner = (Array)arr[i];

                // empty array will be represented as null value
                if (inner.Length == 0) 
                    inner = new object[]{ null };

                foreach( object obj in inner )
                {
                    buffer[i] = obj;
                    UnwindRow(arr, i + 1, ref buffer, ref listResult);
                }
                return;
            }

            listResult.Add( buffer.Clone() as object[] );
        }


    }
}