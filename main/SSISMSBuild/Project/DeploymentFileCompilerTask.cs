using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.DataTransformationServices.Project;
using Microsoft.DataTransformationServices.Project.ComponentModel;
using Microsoft.DataTransformationServices.Project.Serialization;
using Microsoft.DataWarehouse.VsIntegration.Shell.Project.Configuration;
using Microsoft.SqlServer.Dts.Runtime;

namespace Microsoft.SqlServer.IntegrationServices.Build
{	
	/// <summary>
	/// Compiles SSIS Project Deployment files (.ispac) from one or 
	/// more SSIS Visual Studio project files (.dtproj).
	/// </summary>
	public class DeploymentFileCompilerTask : Microsoft.Build.Utilities.Task
	{
		/// <summary>
		/// Path(s) to the SSIS Visual Studio project files (.dtproj) to compile.
		/// </summary>
		[Required]
		public ITaskItem[] InputProject { get; set; }

		/// <summary>
		/// The Visual Studio configuration to use. 
		/// </summary>
		[Required]
		public string Configuration { get; set; }

		/// <summary>
		/// On a successful build, this parameter will be populated with the 
		/// full paths to the project deployment files (.ispac) created during
		/// this build.
		/// </summary>
		[Output]
		public ITaskItem[] CreatedProjects { get; internal set; }

		/// <summary>
		/// (Optional) Sets the protection level for the output project (and all packages). 
		/// If not set, the  protection level specified in the .dtproj is used. Must be a value
		/// from the <see cref="DTSProtectionLevel"/> enum.
		/// </summary>
		public string ProtectionLevel
		{
			get
			{
				return m_protectionLevelString;
			}
			set
			{				 
				if (value != null)
				{
					// try to parse it
					Enum.Parse(typeof (DTSProtectionLevel), value, true);
				}

				m_protectionLevelString = value;
			}
		}

		/// <summary>
		/// (Optional) This property is required when using a protection level that
		/// requires a password.
		/// </summary>
		public string ProjectPassword { get; set; }

		/// <summary>
		/// (Optional) If set, deployment files will be created under this output directory.
		/// If no value is provided, the .ispac file will be created under the Visual Studio
		/// project's directory.
		/// </summary>
		public string RootOutputDirectory { get; set; }

		/// <summary>
		/// (Optional) When set, this version value will be used for the version information in
		/// the .ispac files, overridding any version values set in the .dtproj file.
		/// The format is "&lt;Major>.&lt;Minor>.&lt;Build". Ex: 10.0.1
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// (Optional) This value will populate the <see cref="Project.VersionComments"/> field. 
		/// If no value is provided, the text from the .dtproj file is used.
		/// </summary>
		public string VersionComments { get; set; }

		#region Serialization classes		

		private ProjectSerialization VsProject { get; set; }
		private ProjectManifest Manifest { get; set; }
		private DataTransformationsConfiguration ProjectConfiguration
		{
			get
			{
				if (m_projectConfiguration == null)
				{
					GetProjectConfiguration();
				}

				return m_projectConfiguration;
			}
		}

		private DataTransformationsConfiguration m_projectConfiguration;

		private void GetProjectConfiguration()
		{
			foreach (var c in VsProject.Configurations)
			{
				var config = (DataTransformationsConfiguration)c;
				if (config.Name.Equals(Configuration, StringComparison.OrdinalIgnoreCase))
				{
					m_projectConfiguration = config;
					break;
				}
			}

			if (m_projectConfiguration == null)
			{
				throw new Exception("Configuration not found");
			}
		}

		#endregion

		private string m_protectionLevelString;

		public override bool Execute()
		{
			bool result = true;
			var outputProjects = new List<TaskItem>();

			foreach (var projectFile in InputProject)
			{
				try
				{
					Log.LogMessage("------");

					string projectDirectory = Path.GetDirectoryName(projectFile.ItemSpec);
					string outputDirectory = string.IsNullOrEmpty(RootOutputDirectory) ? projectDirectory : RootOutputDirectory;

					DeserializeProject(projectFile.ItemSpec);

					if (VsProject.DeploymentModel == DeploymentModel.Project)
					{
						// Determine output directory
						string projectOutputPath = GetOutputPath(outputDirectory);
						Log.LogMessage(SR.OutputDirectory(projectOutputPath));

						// Create project and set properties
						var project = Project.CreateProject();
						project.OfflineMode = true;
						SetProjectProperties(project, Manifest);

						// set the protection level
						var protectionLevel = GetProtectionLevel(Manifest);
						Log.LogMessage(SR.ProjectLevel(protectionLevel));
						project.ProtectionLevel = protectionLevel;

						if (PasswordNeeded(protectionLevel))
						{
							if (string.IsNullOrEmpty(ProjectPassword))
							{
								Log.LogError(SR.ProjectPasswordMissing);
								result = false;
								continue;
							}

							project.Password = ProjectPassword;
						}

						// Add parameters to project
						string projectParameterPath = GetProjectParameterPath(projectDirectory);
						var projectParameters = LoadProjectParameters(projectParameterPath);

						foreach (var p in projectParameters.Parameters)
						{
							Log.LogMessage(SR.AddProjectParameter(p.Name));
							var parameter = project.Parameters.Add(p.Name, (TypeCode) Int32.Parse(p.Properties["DataType"]));
							parameter.LoadFromXML(p.GetXml(), new DefaultEvents());
						}

						// Set parameter values from configuration
						var parameterSet = new Dictionary<string, ConfigurationSetting>();
						foreach (string key in ProjectConfiguration.Options.ParameterConfigurationValues.Keys)
						{
							// check if it's a GUID
							Guid guid;
							if (Guid.TryParse(key, out guid))
							{
								var setting = ProjectConfiguration.Options.ParameterConfigurationValues[key];
								parameterSet.Add(key, setting);
							}
						}

						// Add connections to project
						var connectionManagerSerializer = new XmlSerializer(typeof (ProjectConnectionManager));
						foreach (var c in Manifest.ConnectionManagers)
						{
							var path = GetConnectionManagerPath(projectDirectory, c);
							Log.LogMessage(SR.LoadingConnectionManager(path));

							var cmXml = File.ReadAllText(path);
							var connMgr = (ProjectConnectionManager) connectionManagerSerializer.Deserialize(new StringReader(cmXml));

							var cm = project.ConnectionManagerItems.Add(connMgr.CreationName, c.Name);

							cm.Load(null, File.OpenRead(path));
						}

						// Add packages to project
						foreach (var item in Manifest.Packages)
						{
							var packagePath = GetPackagePath(projectDirectory, item);
							var package = LoadPackage(packagePath);

							// check the protection level
							if (package.ProtectionLevel != protectionLevel)
							{
								Log.LogMessage(SR.PackageProtectionLevel(protectionLevel));
								package.ProtectionLevel = protectionLevel;
								if (PasswordNeeded(protectionLevel))
								{
									package.PackagePassword = ProjectPassword;
								}
							}

							// set package parameters
							if (parameterSet.Count > 0)
							{
								SetParameterConfigurationValues(package.Parameters, parameterSet);
							}

							project.PackageItems.Add(package, item.Name);
							project.PackageItems[item.Name].EntryPoint = item.EntryPoint;
						}

						// set project overrides
						var version = GetProjectVersion();
						if (version != null)
						{
							Log.LogMessage(SR.ProjectVersionChange(version));
							project.VersionMajor = version.Major;
							project.VersionMinor = version.Minor;
							project.VersionBuild = version.Build;
						}

						if (VersionComments != null)
						{
							Log.LogMessage(SR.VersionComments);
							project.VersionComments = VersionComments;
						}

						// Save project
						string projectPath = GetProjectFilePath(projectOutputPath, project);
						Log.LogMessage(SR.SavingProject(projectPath));

						project.SaveTo(projectPath);
						project.Dispose();

						// Save the path to the project so it can be used as an output
						outputProjects.Add(new TaskItem(projectPath));
					}
				}
				catch (Exception e)
				{
					Log.LogErrorFromException(e, true);
					result = false;
				}
			}

			CreatedProjects = outputProjects.ToArray();

			return result;
		}

		private bool PasswordNeeded(DTSProtectionLevel level)
		{
			return (level == DTSProtectionLevel.EncryptAllWithPassword ||
			        level == DTSProtectionLevel.EncryptSensitiveWithPassword);
		}

		private void SetParameterConfigurationValues(Parameters parameters, IDictionary<string, ConfigurationSetting> set)
		{
			foreach (Dts.Runtime.Parameter parameter in parameters)
			{
				if (set.ContainsKey(parameter.ID))
				{
					var configSetting = set[parameter.ID];
					parameter.Value = configSetting.Value;

					Log.LogMessage(SR.ConfigSetting(configSetting.Name));

					// remove parameter
					set.Remove(parameter.ID);

					if (set.Count == 0)
					{
						break;
					}
				}
			}
		}

		private string GetOutputPath(string outputDirectory)
		{
			string outputPath = ProjectConfiguration.Options.OutputPath;
			string path = Path.Combine(outputDirectory, outputPath, ProjectConfiguration.Name);
			// make sure it exists
			Directory.CreateDirectory(path);
			return path;
		}

		private DTSProtectionLevel GetProtectionLevel(ProjectManifest manifest)
		{
			var level = manifest.ProtectionLevel;
			if (ProtectionLevel != null)
			{
				level = (DTSProtectionLevel)Enum.Parse(typeof(DTSProtectionLevel), ProtectionLevel, true);
			}

			return level;
		}

		private string GetConnectionManagerPath(string projectDirectory, ConnectionManager connectionManager)
		{
			return Path.Combine(projectDirectory, connectionManager.Name);
		}

		private string GetProjectParameterPath(string projectDirectory)
		{
			return Path.Combine(projectDirectory, "Project.params");
		}

		private ProjectParameters LoadProjectParameters(string file)
		{
			var serializer = new XmlSerializer(typeof(ProjectParameters));
			var fileStream = File.OpenRead(file);
			return (ProjectParameters)serializer.Deserialize(fileStream);
		}

		private void SetProjectProperties(Project project, ProjectManifest manifest)
		{
			// set the properties we care about
			foreach (var prop in manifest.Properties.Keys)
			{
				switch (prop)
				{
					case "Name":
						project.Name = manifest.Properties[prop];
						break;
					case "VersionMajor":
						project.VersionMajor = Int32.Parse(manifest.Properties[prop]);
						break;
					case "VersionMinor":
						project.VersionMinor = Int32.Parse(manifest.Properties[prop]);
						break;
					case "VersionBuild":
						project.VersionBuild = Int32.Parse(manifest.Properties[prop]);
						break;
					case "VersionComments":
						project.VersionComments = manifest.Properties[prop];
						break;
				}
			}
		}

		private string GetPackagePath(string projectDirectory, PackageManifest package)
		{
			return Path.Combine(projectDirectory, package.Name);
		}

		private string GetProjectFilePath(string outputDirectory, Project project)
		{
			string path = Path.Combine(outputDirectory, project.Name);
			return Path.ChangeExtension(path, ".ispac");
		}

		private Version GetProjectVersion()
		{
			Version ver = null;
			if (Version != null)
			{
				ver = new Version(Version);
			}

			return ver;
		}

		private Package LoadPackage(string path)
		{
			Package pkg;

			Log.LogMessage(SR.LoadingPackage(path));
			try
			{
				var xml = File.ReadAllText(path);

				pkg = new Package { IgnoreConfigurationsOnLoad = true, CheckSignatureOnLoad = false, OfflineMode = true };
				pkg.LoadFromXML(xml, null);
			}
			catch (Exception e)
			{
				Log.LogError(SR.ErrorLoadingPackage(path, e.Message));
				throw;
			}

			return pkg;
		}

		private void DeserializeProject(string project)
		{
			Log.LogMessage(SR.LoadingProject(project));

			var xmlOverrides = new XmlAttributeOverrides();
			ProjectConfigurationOptions.PrepareSerializationOverrides(typeof(DataTransformationsProjectConfigurationOptions), SerializationLevel.Project, xmlOverrides);

			// Read project file
			var serializer = new XmlSerializer(typeof(ProjectSerialization), xmlOverrides);
			var fileStream = File.OpenRead(project);
			VsProject = (ProjectSerialization)serializer.Deserialize(fileStream);

			// Read project deployment manifest
			if (VsProject.DeploymentModel == DeploymentModel.Project)
			{
				serializer = new XmlSerializer(typeof(ProjectManifest));
				var reader = new StringReader(VsProject.DeploymentModelSpecificXmlNode.InnerXml);
				Manifest = (ProjectManifest)serializer.Deserialize(reader);

				// TODO: read user settings - do we need to do this for MSBuild?
			}
		}
	}
}
