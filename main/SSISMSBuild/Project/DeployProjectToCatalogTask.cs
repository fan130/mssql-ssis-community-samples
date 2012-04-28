using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.SqlServer.IntegrationServices.Build
{
	/// <summary>
	/// This Task connects to an SSIS Catalog and deploys the given project files.
	/// Ensure that the account running MSBuild has permission to deploy to the catalog.
	/// </summary>
	public class DeployProjectToCatalogTask : Task
	{
		/// <summary>
		/// One or more paths to .ispac deployment files.
		/// </summary>
		[Required]
		public ITaskItem[] DeploymentFile { get; set; }

		/// <summary>
		/// The SQL instance name of the SSIS Catalog to deploy to.
		/// </summary>
		[Required]
		public string Instance { get; set; }

		/// <summary>
		/// The folder on the catalog to deploy to.
		/// If this folder does not exist, it will be created if <see cref="CreateFolder"/> is true.
		/// </summary>
		[Required]
		public string Folder { get; set; }

		/// <summary>
		/// Should the SSIS Catalog Folder be created if it is not already there. 
		/// This property is optional. The default value is true.
		/// </summary>
		public bool CreateFolder { get; set; }

		/// <summary>
		/// The name of the SSIS catalog to deploy to.
		/// This property is optional. The default value is "SSISDB".
		/// </summary>
		public string Catalog { get; set; }

		public DeployProjectToCatalogTask()
		{
			Catalog = "SSISDB";
			CreateFolder = true;
		}

		public override bool Execute()
		{
			bool result = true;
			var csb = new SqlConnectionStringBuilder
			          	{
			          		DataSource = Instance, IntegratedSecurity = true, InitialCatalog = Catalog
			          	};

			Log.LogMessage(SR.ConnectingToServer(csb.ConnectionString));

			using (var conn = new SqlConnection(csb.ConnectionString))
			{
				try
				{
					conn.Open();
				}
				catch (Exception e)
				{
					Log.LogError(SR.ConnectionError);
					Log.LogErrorFromException(e);
					return false;
				}

				foreach (var taskItem in DeploymentFile)
				{
					try
					{
						Log.LogMessage("------");

						string projectPath = taskItem.ItemSpec;

						if (CreateFolder)
						{
							EnsureFolderExists(conn, Folder);
						}

						string projectName = Path.GetFileNameWithoutExtension(projectPath);
						var bytes = File.ReadAllBytes(projectPath);

						var deploymentCmd = GetDeploymentCommand(conn, Folder, projectName, bytes);

						try
						{
							Log.LogMessage(SR.DeployingProject(projectPath));
							deploymentCmd.ExecuteNonQuery();
						}
						catch (Exception)
						{
							Log.LogError(SR.DeploymentFailed);
							throw;
						}
					}
					catch (Exception e)
					{
						Log.LogErrorFromException(e, true);
						result = false;
					}
				}
			}

			return result;
		}

		private void EnsureFolderExists(SqlConnection connection, string folder)
		{
			if (!FolderExists(connection, folder))
			{
				CreateCatalogFolder(connection, folder);
			}
		}

		private static bool FolderExists(SqlConnection connection, string folder)
		{
			var cmd = GetFolderCommand(connection, folder);
			var folderId = cmd.ExecuteScalar();
			return (folderId != null && folderId != DBNull.Value);
		}

		private void CreateCatalogFolder(SqlConnection connection, string folder)
		{
			var cmd = new SqlCommand("[catalog].[create_folder]", connection) {CommandType = CommandType.StoredProcedure};
			cmd.Parameters.AddWithValue("folder_name", folder);

			Log.LogMessage(SR.CreatingFolder(folder));
			cmd.ExecuteNonQuery();
		}

		private static SqlCommand GetFolderCommand(SqlConnection connection, string folder)
		{
			var cmd = new SqlCommand("SELECT folder_id FROM [catalog].[folders] WHERE name = @FolderName", connection);
			cmd.Parameters.AddWithValue("@FolderName", folder);

			return cmd;
		}

		private static SqlCommand GetDeploymentCommand(SqlConnection connection, string folder, string name, byte[] project)
		{
			// build the deployment command
			var cmd = new SqlCommand("[catalog].[deploy_project]", connection) { CommandType = CommandType.StoredProcedure };
			cmd.Parameters.AddWithValue("folder_name", folder);
			cmd.Parameters.AddWithValue("project_name", name);
			cmd.Parameters.AddWithValue("project_stream", project);
			cmd.Parameters.AddWithValue("operation_id", SqlDbType.BigInt).Direction = ParameterDirection.Output;

			return cmd;
		}
	}
}
