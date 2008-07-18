using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

using System.Text.RegularExpressions;

namespace Microsoft.Samples.SqlServer.Dts
{
    [DtsPipelineComponent(
		DisplayName = "RegExFlatFile Source",
        IconResource = "RegExFlatFileSource.TxRegExSrc.ico", // specify an icon for the newly built component 
		Description = "Regular expression based flat file parsing source",
		ComponentType = ComponentType.SourceAdapter)]
	public class RegExFlatFileSource : PipelineComponent
	{
		// Property names
		internal const string s_InputFilePathNamePropName = "InputFilePathName";
		internal const string s_RegExpPropName = "RegularExpression";

		/// <summary>
		/// Adds the custom properties to the component property collection
		/// </summary>
		public override void ProvideComponentProperties()
		{
			base.ProvideComponentProperties();

			IDTSCustomProperty100 regexpValue = ComponentMetaData.CustomPropertyCollection.New();
			regexpValue.Name = s_RegExpPropName;
			regexpValue.Description = "Regular expression used to parse the text file lines";
			regexpValue.Value = "";

			IDTSCustomProperty100 FlatFilePathName = ComponentMetaData.CustomPropertyCollection.New();
			FlatFilePathName.Name = s_InputFilePathNamePropName;
			FlatFilePathName.Description = "Full path to the text file used as input";
			FlatFilePathName.Value = "";
            // make the property expressionable. 
            FlatFilePathName.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            FlatFilePathName.UITypeEditor = "System.Windows.Forms.Design.FileNameEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

			// Remove the default input and output provided to all pipeline components
			ComponentMetaData.InputCollection.RemoveAll();
			ComponentMetaData.OutputCollection.RemoveAll();

			// Add the 2 outputs used by this component:
			// Matching output controled by the regular expression content
			IDTSOutput100 output = ComponentMetaData.OutputCollection.New();
			output.Name = "Matching output";

			//  Non-matching output where all the text lines that did not match the regular expression are redirected
			output = ComponentMetaData.OutputCollection.New();
			output.Name = "Non matching output";
			IDTSOutputColumn100 col = output.OutputColumnCollection.New();
			col.SetDataTypeProperties(DataType.DT_STR, 8000, 0, 0, 1252);
			col.Name = "Unmatched";
			col.Description = "Unmatched text line";
		}

		/// <summary>
		/// Syncronizes the matching output columns with the groups defined in the user supplied regular expression:
		///   e.g. regular expression ([a-z]+)123([A-W]+) contains 3 groups:
		///		1st group that contains the entire expression
		///		2nd group that contains the match for [a-z]+
		///		3rd group that contains the match for [A-W]+
		/// </summary>
		private void SyncOutputWithRegexGroups()
		{
            // Remove the existing columns from the matching output collection
            IDTSOutput100 output = ComponentMetaData.OutputCollection[0];
            output.OutputColumnCollection.RemoveAll();

            // if there is no regular expression specified don't do anything, the component will not pass validation.
			string regexpval = ComponentMetaData.CustomPropertyCollection[s_RegExpPropName].Value.ToString();
			if (String.IsNullOrEmpty(regexpval))
			{
				return;
			}

			// Get the groups defined by the regular expression
			Regex rx = new Regex(regexpval);
			string[] rxgrpnames = rx.GetGroupNames();

			IDTSOutputColumn100 col;
            if (null == rxgrpnames)
			{
				return;
			}

            if (0 == rxgrpnames.GetLength(0))
			{
				return;
			}

            // For each group defined in the regular expression add an output column
            // with data type set to ansi string
			foreach (string grpname in rxgrpnames)
			{
				col = output.OutputColumnCollection.New();
				col.SetDataTypeProperties(DataType.DT_STR, 8000, 0, 0, 1252);
                col.Name = grpname;
			}
		}

		/// <summary>
		/// Checks to see if the column metadata collection has different column names/numbers than those names passed in as parameter
		/// </summary>
		/// <param name="newcols"></param>
		/// <returns></returns>
		private bool ColumnMetadataChanged(string[] newcols)
		{
			IDTSOutputColumnCollection100 outputcolumns = ComponentMetaData.OutputCollection[0].OutputColumnCollection;
			if (outputcolumns.Count != newcols.GetLength(0))
			{
				return true;
			}
			else
			{
				for (int n = 0; n < newcols.GetLength(0); n++)
				{
					if (outputcolumns[n].Name != newcols[n])
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Called by the DataFlow task to validate the transform
		/// </summary>
		/// <returns></returns>
        public override DTSValidationStatus Validate()
        {
            IDTSCustomProperty100 rxPropExpression = ComponentMetaData.CustomPropertyCollection[s_RegExpPropName];

			// If there is no regular expression or if the input file does not exist we mark the component broken and output an error.
            if( (rxPropExpression == null) || (String.IsNullOrEmpty(rxPropExpression.Value.ToString())) )
			{
				bool cancel;
				ComponentMetaData.FireError(HResults.DTS_E_VALIDATIONFAILED, s_RegExpPropName, "Specify a non-empty regular expression!", null, 0, out cancel);
				return DTSValidationStatus.VS_ISBROKEN;
			}
			// check if the input file exists...
			else if (System.IO.File.Exists(ComponentMetaData.CustomPropertyCollection[s_InputFilePathNamePropName].Value.ToString()))
			{
				// Check to see if the number and/or name of columns changed based on the regular expression property
				// If it did change than return DTSValidationStatus.VS_NEEDSNEWMETADATA to request that ReinitializeMetaData is called to update the component metadata
                Regex rx = new Regex(rxPropExpression.Value.ToString());
				string[] grps = rx.GetGroupNames();

				// If the regular expression and the matching output are out of sync
				// we return DTSValidationStatus.VS_NEEDSNEWMETADATA to trigger a call to ReinitializeMetadata
				if( ColumnMetadataChanged(grps) )
				{
					return DTSValidationStatus.VS_NEEDSNEWMETADATA;
				}
				return base.Validate();
			}
			else
			{
				bool cancel;
				ComponentMetaData.FireError(HResults.DTS_E_VALIDATIONFAILED, s_InputFilePathNamePropName, "Specify an existing file!", null, 0, out cancel);
				return DTSValidationStatus.VS_ISBROKEN;
			}
        }

		/// <summary>
		/// Called when the Validate returns DTSValidationStatus.VS_NEEDSNEWMETADATA and refreshes the component metadata
		/// </summary>
		public override void ReinitializeMetaData()
		{
			SyncOutputWithRegexGroups();
			base.ReinitializeMetaData();
		}

		/// <summary>
		/// The main unit of work of a Source component, populates the pipeline buffer with data.
		/// </summary>
		/// <param name="outputs"></param>
		/// <param name="outputIDs"></param>
		/// <param name="buffers"></param>
		public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
		{
            try
            {
                Regex rx = new Regex(ComponentMetaData.CustomPropertyCollection[s_RegExpPropName].Value.ToString());
                Match match = null;

                using (System.IO.TextReader reader = System.IO.File.OpenText(ComponentMetaData.CustomPropertyCollection[s_InputFilePathNamePropName].Value.ToString()))
                {
                    string crtLine = reader.ReadLine();

                    while (null != crtLine)
                    {
                        match = rx.Match(crtLine);
                        if (match.Success)
                        {
                            // split the text based on the groups in the regular expression and send it to the matching output
                            buffers[0].AddRow();
                            for (int nIdx = 0; nIdx < buffers[0].ColumnCount; nIdx++)
                            {
                                buffers[0].SetString(nIdx, match.Groups[nIdx].Value);
                            }
                        }
                        else
                        {
                            // direct the text to the non-matching output
                            buffers[1].AddRow();
                            buffers[1].SetString(0, crtLine);
                        }
                        crtLine = reader.ReadLine();
                    }

                    reader.Close();
                }

                // mark the pipeline buffers (the matching and non matching ones) as completed.
                buffers[0].SetEndOfRowset();
                buffers[1].SetEndOfRowset();
            }
            catch (Exception ex)
            {
                bool cancel;
                ComponentMetaData.FireError(HResults.DTS_E_PRIMEOUTPUTFAILED, null, ex.Message, null, 0, out cancel);
            }
		}
    }
}
