using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;


[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(
   SecurityAction.RequestMinimum, Execution = true)]

namespace Microsoft.Samples.SqlServer.SSIS.PackageGeneration
{
	static class ExampleCreateRunPackage
	{
		static SimplePackageGenerator packageGeneration;
		static bool createNewDest;
		static bool mapColumn;
		static Hashtable colPairTable;
		static String packagePath;
		
		static void Main(String[] args)
		{
			packageGeneration = new SimplePackageGenerator();

			bool parseOk = false;
			try
			{
				parseOk = Parse(args);
			}
			catch (Exception e)
			{
				Console.WriteLine("Parsing command line has failed because: " + e.Message);
			}

			if (!parseOk)
			{
				StringBuilder sb = new StringBuilder("\n\nUsage: PkgGen -[SQL|Excel|File] SourceValue -[SQL|Excel|File] DestinationValue -C[REATENEW] {ON|OFF} -M[APCOLUMN] {ON|OFF} [-SAVE SSISPackagePath]\n\n");
				sb.AppendLine("The value for each source and destination type is:");
				sb.AppendLine("-SQL    Host Name; Database Name; Table Name");
				sb.AppendLine("-Excel  Excel File Path; Sheet Name; {true|false}");
				sb.AppendLine("-File   Flat File Path; {true|false};");
				sb.AppendLine("        {tab|comma|semicolon|colon|verticalbar};");
				sb.AppendLine("        {ColumnName,Datatype[,Length][,Precision][,Scale];}(0+)");
				sb.AppendLine("See Readme for more details of options and examples");
				Console.WriteLine(sb.ToString());
				return;
			}
			
			
			try
			{
				packageGeneration.ConstructPackage(createNewDest);
			}
			catch (Exception e)
			{
				Console.WriteLine("\nFailed to consturct package, the error message returned is: " + e.Message);
                return;
			}

            if (mapColumn)
            {
                colPairTable = null;
                if (!AskForMatchMap()) return;
               
            }

            packageGeneration.AddPathsAndConnectColumns(colPairTable);

			if (!packageGeneration.ValidatePackage())
			{
                Console.WriteLine("\nFailed to validate the package, you can check the generated package in Business Intelligence Studio if it has been saved.");
				if (!String.IsNullOrEmpty(packagePath))
				{
					packageGeneration.SavePackage(packagePath);
				}
				return;
			}

			if (!String.IsNullOrEmpty(packagePath))
			{
				packageGeneration.SavePackage(packagePath);
				Console.WriteLine("\nSql Server Integration Services Package has been saved to "+packagePath);
			}

			String errors;
			if (packageGeneration.ExecutePackage(out errors))
			{
				Console.WriteLine("\nTransfer has succeeded");
			}
			else
			{
                Console.WriteLine("\nTransfer has failed. You can open the generated package in Business Intelligence Studio if it has been saved and debug there");
				Console.WriteLine(errors);
			}
            
		}


		private static bool Parse(String[] args)
		{
			if (args.Length != 8 && args.Length != 10)
			{
				return false;
			}
			if (!ParseConnectionTypeValue(args[0], args[1], true))
			{
				return false;
			}
			if (!ParseConnectionTypeValue(args[2], args[3], false))
			{
				return false;
			}
			
			if (!args[4].Equals("-CREATENEW", StringComparison.OrdinalIgnoreCase)
				&& !args[4].Equals("-C", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Expecting \"-C[REATENEW]\" while the actual input is " + args[4]);
				return false;
			}
			createNewDest = args[5].Equals("ON", StringComparison.OrdinalIgnoreCase);
			
			if (!args[6].Equals("-MAPCOLUMN", StringComparison.OrdinalIgnoreCase)
				&& !args[6].Equals("-M", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("Expecting \"-M[APCOLUMN]\" while the actual input is " + args[6]);
				return false;
			}
			mapColumn = args[7].Equals("ON", StringComparison.OrdinalIgnoreCase);

			if (args.Length == 10)
			{
				if (!args[8].Equals("-SAVE", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Expecting \"-SAVE\" while the actual input is " + args[8]);
					return false;
				}
				packagePath = args[9];
			}
			return true;
		}

		private static bool ParseConnectionTypeValue(string option, string value, bool isSource)
		{
			bool parseOk;
			if (String.Compare(option, "-SQL", StringComparison.OrdinalIgnoreCase) == 0)
			{
				String hostName, cataLog, table, connectionString;
				parseOk = ParseSqlString(value, out hostName, out cataLog, out table);
				if (!parseOk) return false;
				connectionString = String.Format(CultureInfo.InvariantCulture, "Data Source={0};Initial Catalog={1};Provider=SQLNCLI10;Integrated Security=SSPI;Auto Translate=false;", hostName, cataLog);
				if (isSource)
				{
                    packageGeneration.SourceProvider = new SqlConnectionTypeProvider(true, table, connectionString, "[", "]");
				}
				else
				{
                    packageGeneration.DestProvider = new SqlConnectionTypeProvider(false, table, connectionString, "[", "]");
				}
			}
			else if (String.Compare(option, "-FILE", StringComparison.OrdinalIgnoreCase) == 0)
			{
				String fileName;
				bool columnNamesinFirstRow;
				String columnDelimiter;
				String[] colNames;
				DataType[] dataTypes;
				int[] lengths;
				int[] precisions;
				int[] scales;

				parseOk = ParseFlatFileString(value, out fileName, out columnNamesinFirstRow, out columnDelimiter, out colNames, out dataTypes,
					out lengths, out precisions, out scales);
				if (!parseOk) return false;
				if (isSource)
				{
					packageGeneration.SourceProvider = new FlatFileConnectionTypeProvider(true, fileName, columnNamesinFirstRow, columnDelimiter,
                        colNames, dataTypes, lengths, precisions, scales);
				}
				else
				{
					packageGeneration.DestProvider = new FlatFileConnectionTypeProvider(false, fileName, columnNamesinFirstRow, columnDelimiter,
                        colNames, dataTypes, lengths, precisions, scales);
				}
			}
			else if (String.Compare(option, "-EXCEL", StringComparison.OrdinalIgnoreCase) == 0)
			{
				String excelFileName, excelSheetName, connectionString;
				bool firstRowHasColName;
				parseOk = ParseExcelString(value, out excelFileName, out excelSheetName, out firstRowHasColName);
				if (!parseOk) return false;
				connectionString = String.Format(CultureInfo.InvariantCulture, "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=YES\";", excelFileName);
				if (isSource)
				{
                    packageGeneration.SourceProvider = new ExcelConnectionTypeProvider(true, excelFileName, excelSheetName, connectionString, firstRowHasColName, "`", "`");
				}
				else
				{
					packageGeneration.DestProvider = new ExcelConnectionTypeProvider(false, excelFileName, excelSheetName, connectionString, firstRowHasColName, "`", "`");
				}
			}
			else
			{
				Console.WriteLine("\nInvalid option, should be one of -SQL, -EXCEL and -FILE.");
				return false;
			}
			return true;
		}

		
		private static bool ParseSqlString(String value, out String hostName, out String cataLog, out String tableName)
		{
			hostName = String.Empty;
			cataLog = String.Empty;
			tableName = String.Empty;
			String[] splitted = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (splitted.Length != 3)
			{
				Console.WriteLine("The correct sql string format is {host name}:{initial catalog}:{table name}.");
				return false;
			}

			hostName = splitted[0];
			cataLog = splitted[1];
			tableName = splitted[2];
			return true;
		}

		private static bool ParseFlatFileString(string value, out string fileName, out bool columnNamesInFirstRow, out string columnDelimiter, 
			out string[] colNames, out DataType [] dataTypes, out int[] lengths, out int[] precisions, out int[] scales)
		{
			fileName = String.Empty;
			columnNamesInFirstRow = false;
			columnDelimiter = string.Empty;
			colNames = new String[0];
			dataTypes = new DataType[0];
			lengths = new int[0];
			precisions = new int[0];
			scales = new int[0];

			String[] splitted = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (splitted.Length < 3)
			{
				Console.WriteLine("wrong flat file string");
				return false;
			}
			fileName = splitted[0];
			if (String.Compare(splitted[1], "true", StringComparison.OrdinalIgnoreCase)==0)
			{
				columnNamesInFirstRow = true;
			}
			else if (String.Compare(splitted[1], "false", StringComparison.OrdinalIgnoreCase) == 0)
			{
				columnNamesInFirstRow = false;
			}
			else
			{
				Console.WriteLine("Expecting the word true or false.");
				return false;
			}

			String delimiter = splitted[2].ToUpper(CultureInfo.InvariantCulture);
			switch (delimiter)
			{
				case "TAB":
					columnDelimiter = "\t"; break;
				case "COMMA":
					columnDelimiter = ","; break;
				case "COLON":
					columnDelimiter = ":"; break;
				case "SEMICOLON":
					columnDelimiter = ";"; break;
				case "VERTICALBAR":
					columnDelimiter = "|"; break;
				default:
					Console.WriteLine("Exepct one of the value: tab, comma, semicolon, colon or verticalbar for column delimiter.");
					return false;
					
			}

			int numCols = splitted.Length - 3;		
			colNames = new String[numCols];
			dataTypes = new DataType[numCols];
			lengths = new int[numCols];
			precisions = new int[numCols];
			scales = new int[numCols];

			for (int i = 0; i < numCols; i++)
			{
				int argIndex = i+3;
				String[] temp = splitted[argIndex].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); ;
				if (temp.Length < 2)
				{
					Console.WriteLine("Expecting at least column name and data type separated by comma.");
					return false;
				}
				colNames[i] = temp[0];
                temp[1] = temp[1].ToUpper(CultureInfo.InvariantCulture);
				switch (temp[1])
				{
					case "DT_I4": 
						dataTypes[i] = DataType.DT_I4; 
						break;
					case "DT_I8": 
						dataTypes[i] = DataType.DT_I8;
						break;
					case "DT_R4":
						dataTypes[i] = DataType.DT_R4;
						try
						{
							precisions[i] = Int32.Parse(temp[2], CultureInfo.InvariantCulture);
							scales[i] = Int32.Parse((temp[3]), CultureInfo.InvariantCulture);
						}
						catch
						{
							Console.WriteLine("Expecting colname,DT_R4,precision,scale for column " + splitted[argIndex]);
							return false;
						}
						break;
					case "DT_R8": 
						dataTypes[i] = DataType.DT_R8;
						try
						{
							precisions[i] = Int32.Parse(temp[2], CultureInfo.InvariantCulture);
							scales[i] = Int32.Parse(temp[3], CultureInfo.InvariantCulture);
						}
						catch
						{
							Console.WriteLine("Expecting colname,DT_R8,precision,scale for column " + splitted[argIndex]);
							return false;
						}
						break;
					case "DT_STR": 
						dataTypes[i] = DataType.DT_STR;
						try
						{
							lengths[i] = Int32.Parse(temp[2], CultureInfo.InvariantCulture);
						}
						catch
						{
							Console.WriteLine("Expect colname,DT_STR,length for column " + splitted[argIndex]);
							return false;
						}
						break;
					case "DT_DECIMAL": 
						dataTypes[i] = DataType.DT_DECIMAL; 
						break;
					case "DT_DBTIMESTAMP": 
						dataTypes[i] = DataType.DT_DBTIMESTAMP; 
						break;
					default: 
						Console.WriteLine("Unexpected pipeline data type entered: " + temp[1]);
						return false;
				}
			}

			return true;
		}

	

		private static bool ParseExcelString(String value, out String excelFileName, out String sheetName, out bool firstRowColName)
		{
			excelFileName = String.Empty;
			sheetName = String.Empty;
			firstRowColName = true;
			String[] splitted = value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (splitted.Length != 3)
			{
				Console.WriteLine("wrong Excel file string");
				return false;
			}
			excelFileName = splitted[0];
			if (splitted[1][0].Equals('['))
			{
				sheetName = splitted[1].Substring(1, splitted[1].Length - 2);
			}
			sheetName = splitted[1];
			firstRowColName = splitted[2].Equals("TRUE", StringComparison.OrdinalIgnoreCase);
			return true;
		}
		
		private static bool AskForMatchMap()
		{
			String [] srcColNames;
			String [] destColNames;
            
            try
            {
                // Get list of column names of source.
                srcColNames = packageGeneration.SourceProvider.GetColNames();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get columns from source, an exception has been raised: " + e.Message);
                return false;
            }

            try
            {
                // Get list of column names of destination.
                destColNames = packageGeneration.DestProvider.GetColNames();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get columns from destination, an exception has been raised: " + e.Message);
                return false;
            }


			Console.WriteLine("The list of data source columns are:\n");
			foreach (String name in srcColNames)
			{
				Console.WriteLine(name);
			}
			Console.WriteLine("\nThe list of data destination columns are:\n");
			for (int index = 0; index < destColNames.Length; index++)
			{
				Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "[{0}]: {1}", index, destColNames[index]));
			}
			Console.WriteLine("\nEnter the index number in front of the above destination column for each source column, or nothing to skip\n");
			colPairTable = new Hashtable(srcColNames.Length);
			foreach (String name in srcColNames)
			{
				Console.Write(String.Format(CultureInfo.InvariantCulture, "{0} : ", name));

				int index = -1;
				try
				{
					index = Convert.ToInt32(Console.ReadLine(),CultureInfo.InvariantCulture);
				}
				catch (Exception)
				{
				}
				if (index < 0 || index >= destColNames.Length)
				{
					continue;
				}
				colPairTable[name] = destColNames[index];
			}
			Console.WriteLine("\nYou have matched the following columns:\n");
			foreach (object key in colPairTable.Keys)
			{
				Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}\t: {1}", key, colPairTable[key]));
			}
			Console.WriteLine("\nType Q to abort if you are not satisfied with the mapping");
			String typedWord = Console.ReadLine();
			if (typedWord.Equals("q", StringComparison.OrdinalIgnoreCase)) return false;
			return true;

		}

		

	}

}

