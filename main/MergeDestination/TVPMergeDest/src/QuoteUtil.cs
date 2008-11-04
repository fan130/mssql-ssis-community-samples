using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using Microsoft.SqlServer.Management.Diagnostics;



namespace Microsoft.SqlServer.Dts.Pipeline
{
	/// <summary> This class is used by ADO.NET destination component to 
	/// deal with quoting database object names with provider-specific literals
	/// </summary>
	public partial class TVPMergeDestination
	{
		private class QuoteUtil
		{
			// use either brackets or double quotes
			private string prefix;
			private string suffix;
			private string separator;

			// . [ ] are regular expression patterns, they need to be escaped. " does not.
			private string escapedPrefix;
			private string escapedSuffix;
			private string escapedSeparator;

			// database object ID composed by a simple word, it does not need to be quoted
			// for example dbo.employee has two simple word IDs dbo and employee
			string SimpleWordID = @"[\w\$]+";

			// database object ID including special characters, which needs to be quoted
			// for example dbo.[software manufacturs] has a simple word ID dbo and a 
			// special word ID "software manufacturs" quoted by brackets.
			// in case of double quotes, " needs to be escaped by itself.
			string SpecialWordID_DoubleQuotes = @"([\w \^\[\]\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<=\>\?\|\#]|"""")+";
			// in case of hardbracket, right bracket ] needs to be escaped by itself.
			string SpecialWordID_HardBrackets = @"([\w \^\[""\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<=\>\?\|\#]|\]\])+";
			string SpeicalWordID = null;
			// CombinedID is either a SimpleWordID or a quoted SpecialWordID
			// CombinedID is used to represent a token of a database object name 
			// which can be either of the two. For example, the first token of 
			// dbo.[software manufacturs] is a SimpleWordID and the second is a 
			// quoted SpecialWordID
			
			// constants
			const string TABLENAMELVL3 = "TABLENAMELVL3";
			const string TABLENAMELVL2 = "TABLENAMELVL2";
			const string TABLENAMELVL1 = "TABLENAMELVL1";


			public String Prefix
			{
				get { return prefix; }
			}

			public String Sufix
			{
				get { return suffix; }
			}

			// Constructor. 
			// The fields of this class is set up according to the type of the connection
			public QuoteUtil(DbConnection connection)
			{
				// default is double quotes. 
				// currently only oledb provide quotes information, others will use default
				prefix = "\"";
				suffix = "\"";
				separator = ".";

				// connection must be open in order to initialize this class
				STrace.Assert(connection.State == ConnectionState.Open, "the connection is not open");

				System.Data.OleDb.OleDbConnection oleDbConnection =
					connection as System.Data.OleDb.OleDbConnection;
				if (oleDbConnection != null )
				{
					// special case SQL Server
					string connectionString = connection.ConnectionString;
					string providerToken = string.Empty;
					string[] tokens = connectionString.Split(new char[] { ';' });
					foreach (string token in tokens)
					{
						// navigate to "Provider = "
						if (token.Trim().StartsWith("Provider",
							StringComparison.OrdinalIgnoreCase))
						{
							if (token.IndexOf("SQLOLEDB",
								StringComparison.OrdinalIgnoreCase) != -1 ||
								token.IndexOf("SQLNCLI",
								StringComparison.OrdinalIgnoreCase) != -1)
							{
								prefix = "[";
								suffix = "]";
								escapedPrefix = "\\[";
								escapedSuffix = "\\]";
								escapedSeparator = "\\.";
								SpeicalWordID = SpecialWordID_HardBrackets;
								return;
							}
						}
					}

					DataTable literalTable = oleDbConnection.GetOleDbSchemaTable(
						OleDbSchemaGuid.DbInfoLiterals, null);

					if (literalTable != null)
					{
						DataRow[] rows = literalTable.Select("LiteralName = 'Quote_Prefix'");

						if (rows.Length > 0)
						{
							prefix = rows[0]["LiteralValue"].ToString();
							if (prefix == null)
							{
								prefix = string.Empty;
							}
						}
						else
						{
							prefix = string.Empty;
						}

						rows = literalTable.Select("LiteralName = 'Quote_Suffix'");
						if (rows.Length > 0)
						{
							suffix = rows[0]["LiteralValue"].ToString();
							if (suffix == null)
							{
								suffix = string.Empty;
							}
						}
						else
						{
							suffix = string.Empty;
						}

						rows = literalTable.Select("LiteralName = 'Schema_Separator'");
						if (rows.Length > 0)
						{
							separator = rows[0]["LiteralValue"].ToString();
							if (separator == null)
							{
								separator = string.Empty;
							}
						}
						else
						{
							separator = string.Empty;
						}
					}
				}
				// escape those symbols which are meaningful in regular expression, such as [].
				escapedPrefix = prefix;
				escapedSuffix = suffix;
				escapedSeparator = separator;
				if (prefix == "[")
				{
					escapedPrefix = "\\[";
					escapedSuffix = "\\]";
					SpeicalWordID = SpecialWordID_HardBrackets;
				}
				else
				{
					SpeicalWordID = SpecialWordID_DoubleQuotes;
				}
				if (separator == ".")
				{
					escapedSeparator = "\\.";
				}
				
			}

			// QuoteName is used to quote database object names that need to be quoted
			public string QuoteName(string name)
			{
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				{
					return name;
				}
				else if (suffix.CompareTo("]") == 0)
				{
					return prefix + name.Replace("]", "]]") + "]";
				}
				else if (suffix.CompareTo("\"") == 0)
				{
					return prefix + name.Replace("\"", "\"\"") + suffix;
				}
				else 
				{
					return prefix + name + suffix;
				}
			}


			// if the syntax of the original multipart table name is wrong, return false
			// otherwise return each part of the original name.
			public bool GetValidTableName(string tableName, out string tableNameLvl3, out string tableNameLvl2, out string tableNameLvl1)
			{
				tableNameLvl3 = null;
				tableNameLvl2 = null;
				tableNameLvl1 = null;
			
				string TableNameLvl3Pattern = @"(" + TagSimplePattern(SimpleWordID, TABLENAMELVL3) + @"|"
					+ TagSpecialPattern(SpeicalWordID, TABLENAMELVL3) + @")";
				string TableNameLvl2Pattern = @"(" + TagSimplePattern(SimpleWordID, TABLENAMELVL2) + @"|"
					+ TagSpecialPattern(SpeicalWordID, TABLENAMELVL2) + @")";
				string TableNameLvl1Pattern = @"(" + TagSimplePattern(SimpleWordID, TABLENAMELVL1) + @"|"
					+ TagSpecialPattern(SpeicalWordID, TABLENAMELVL1) + @")";
				
				Match match;
				string stringPattern = null;
				// case 1 : lvl2.lvl1 (most common)
				stringPattern = @"\s*" + TableNameLvl2Pattern + escapedSeparator + TableNameLvl1Pattern + @"\s*";
				match = Regex.Match(tableName, stringPattern,
					RegexOptions.IgnoreCase | RegexOptions.Compiled);

				if (match.Success && (match.Length == tableName.Length))
				{
					tableNameLvl2 = match.Groups[TABLENAMELVL2].Value;
					tableNameLvl1 = match.Groups[TABLENAMELVL1].Value;
					return true;
				}
				// case 2 : lvl1
				stringPattern = @"\s*" + TableNameLvl1Pattern + @"\s*";
				match = Regex.Match(tableName, stringPattern,
					RegexOptions.IgnoreCase | RegexOptions.Compiled);

				if (match.Success && (match.Length == tableName.Length))
				{
					tableNameLvl1 = match.Groups[TABLENAMELVL1].Value;
					return true;
				}
				// case 3 : lvl3.lvl2.lvl1
				stringPattern = @"\s*" + TableNameLvl3Pattern + escapedSeparator +
					TableNameLvl2Pattern + escapedSeparator + TableNameLvl1Pattern + @"\s*";
				match = Regex.Match(tableName, stringPattern,
					RegexOptions.IgnoreCase | RegexOptions.Compiled);

				if (match.Success && (match.Length == tableName.Length))
				{
					tableNameLvl3 = match.Groups[TABLENAMELVL3].Value;
					tableNameLvl2 = match.Groups[TABLENAMELVL2].Value;
					tableNameLvl1 = match.Groups[TABLENAMELVL1].Value;
					return true;
				}
				return false;

			}

			private string TagSpecialPattern(string SpeicalWordID, string tag)
			{
				return escapedPrefix + @"(?<" + tag + @">" + SpeicalWordID + @")" + escapedSuffix;
			}

			private string TagSimplePattern(string SimpleWordID, string tag)
			{
				return @"(?<" + tag + @">" + SimpleWordID + @")";
			}
			
		}
	}
}
