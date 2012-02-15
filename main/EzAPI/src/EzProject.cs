using System;
using System.IO;
using Microsoft.SqlServer.Dts.Runtime;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.SqlServer.SSIS.EzAPI
{
    /// <summary>
    /// EzProject is a base class which will be inherited by other types project.
    /// EzProject provides a set of wrapper methods to create project, add packages,
    /// and set the project properties.
    /// </summary>
    public class EzProject
    {
        #region members & properties
        private Project m_project;

        /// <summary>
        /// Get Project instance
        /// </summary>
        public Project Project
        {
            get
            {
                if (null == m_project)
                {
                    m_project = Project.CreateProject();
                }

                return m_project;
            }
        }

        /// <summary>
        /// Get packages in the project
        /// </summary>
        public PackageItems PackageItems
        {
            get
            {
                return this.Project.PackageItems;
            }
        }

        /// <summary>
        /// Get ConnectionManagerItems in the Project
        /// </summary>
        public ConnectionManagerItems ConnectionManagerItems
        {
            get
            {
                return this.Project.ConnectionManagerItems;
            }
        }

        /// <summary>
        /// Get parameters in the project
        /// </summary>
        public Parameters Parameters
        {
            get
            {
                return this.Project.Parameters;
            }
        }

        /// <summary>
        /// Set password of project
        /// </summary>
        public string Password
        {
            set
            {
                this.Project.Password = value;
            }
        }

        /// <summary>
        /// Get, set creation date of project
        /// </summary>
        public DateTimeOffset CreationDate
        {
            get { return this.Project.CreationDate; }
            set { this.Project.CreationDate = value; }
        }

        /// <summary>
        /// Get, set creator name of project
        /// </summary>
        public string CreatorName
        {
            get { return this.Project.CreatorName; }
            set { this.Project.CreatorName = value; }
        }

        /// <summary>
        /// Get description of project
        /// </summary>
        public string Description
        {
            get { return this.Project.Description; }
            set { this.Project.Description = value; }
        }

        /// <summary>
        /// Get ID of project
        /// </summary>
        public string ID
        {
            get { return this.Project.ID; }
        }

        /// <summary>
        /// Get, set name of project
        /// </summary>
        public string Name
        {
            get { return this.Project.Name; }
            set { this.Project.Name = value; }
        }

        /// <summary>
        /// Get, set protection level of project
        /// </summary>
        public DTSProtectionLevel ProtectionLevel
        {
            get { return this.Project.ProtectionLevel; }
            set { this.Project.ProtectionLevel = value; }
        }

        /// <summary>
        /// Get, set version build of project
        /// </summary>
        public int VersionBuild
        {
            get { return this.Project.VersionBuild; }
            set { this.Project.VersionBuild = value; }
        }

        /// <summary>
        /// Get, set version comments of project
        /// </summary>
        public string VersionComments
        {
            get { return this.Project.VersionComments; }
            set { this.Project.VersionComments = value; }
        }

        /// <summary>
        /// Get, set version major of project
        /// </summary>
        public int VersionMajor
        {
            get { return this.Project.VersionMajor; }
            set { this.Project.VersionMajor = value; }
        }

        /// <summary>
        /// Get, set verion minor of project
        /// </summary>
        public int VersionMinor
        {
            get { return this.Project.VersionMinor; }
            set { this.Project.VersionMinor = value; }
        }

        #endregion

        #region constructor & operator override
        public EzProject()
        {
            this.m_project = Project.CreateProject();
        }

        public EzProject(string projectfile)
        {
            this.m_project = Project.CreateProject(projectfile);
        }

        /// <summary>
        /// Construct an EzProject from a Project Object
        /// </summary>
        /// <param name="project"></param>
        public EzProject(Project project)
        {
            if (null == project)
            {
                this.m_project = Project.CreateProject();
            }
            else
            {
                this.m_project = project;
            }
        }

        /// <summary>
        /// override operator, in order to return a Project object from an EzProject
        /// </summary>
        /// <param name="project">operand of EzProject object</param>
        /// <returns>Return a Project object</returns>
        public static implicit operator Project(EzProject project)
        {
            if (project == null)
            {
                return null;
            }

            return (Project)project.Project;
        }

        /// <summary>
        /// override operator, in order to return an EzProject object from a Project
        /// </summary>
        /// <param name="project">oeprand of Project object</param>
        /// <returns>Return an EzProject object</returns>
        public static implicit operator EzProject(Project project)
        {
            if (project == null)
            {
                return null;
            }

            return new EzProject(project);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add an EzPackage to project
        /// </summary>
        /// <typeparam name="T">EzPackage constraint</typeparam>
        /// <param name="package">EzPackage object</param>
        /// <returns>package stream name</returns>
        public string AddPackage<T>(T package) where T : EzPackage
        {
            Debug.Assert(null != package, @"The package should not be nullable.");
            string packageStreamName = package.Name + ".dtsx";
            this.Project.PackageItems.Add((Package)package, packageStreamName);
            return packageStreamName;
        }

        /// <summary>
        /// Add a Package to project
        /// </summary>
        /// <param name="package"></param>
        /// <returns>package stream name</returns>
        public string AddPackage(Package package)
        {
            Debug.Assert(null != package, @"The package should not be nullable.");
            this.Project.PackageItems.Add(package, package.Name + ".dtsx");
            return package.Name;
        }

        /// <summary>
        /// Add a package to project with a specified stream name
        /// </summary>
        /// <param name="package"></param>
        /// <param name="streamname"></param>
        /// <returns>package stream name</returns>
        public string AddPackage(Package package, string streamname)
        {
            Debug.Assert(null != package, @"The package should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(streamname), @"The stream name should not be nullable.");

            this.Project.PackageItems.Add(package, streamname);
            return streamname;
        }

        /// <summary>
        /// Insert a package at a specified position
        /// </summary>
        /// <param name="index">the index to specify the package in the project</param>
        /// <param name="package">the package object</param>
        /// <returns>package stream name</returns>
        public string InsertPackage(int index, Package package)
        {
            Debug.Assert(index > -1, @"The package index must be greater than or equal to 0.");
            Debug.Assert(null != package, @"The package should not be nullable.");

            try
            {
                this.Project.PackageItems.Insert(index, package, package.Name);
                return package.Name;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Remove a package at specified index position
        /// </summary>
        /// <param name="index">the index to specify the package in project</param>
        public void RemovePackageAt(int index)
        {
            Debug.Assert(index > -1, @"The package index must be greater than or equal to 0.");

            this.Project.PackageItems.RemoveAt(index);
        }

        /// <summary>
        /// Remove a package by specifiying stream name
        /// </summary>
        /// <param name="packagename">package stream name</param>
        public void RemovePackage(string packagename)
        {
            Debug.Assert(!string.IsNullOrEmpty(packagename), @"The stream name should not be nullable.");

            this.Project.PackageItems.Remove(packagename);
        }

        /// <summary>
        /// Add an EzConnectionManager to Project
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public ConnectionManager AddConnectionManager<T>(T connection) where T : EzConnectionManager
        {
            Debug.Assert(null != connection, @"The package should not be nullable.");
            string connectionStreamName = connection.Name + ".conmgr";
            ConnectionManagerItem cmi = this.Project.ConnectionManagerItems.Add(connection.GetConnMgrID(), connectionStreamName);
            return cmi.ConnectionManager;
        }

        /// <summary>
        /// Add a connection to Project
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public ConnectionManager AddConnectionManager(ConnectionManager connection)
        {
            Debug.Assert(null != connection, @"The connection should not be nullable.");
            string connectionStreamName = connection.Name + ".conmgr";
            ConnectionManagerItem cmi = this.Project.ConnectionManagerItems.Add(connection.CreationName, connectionStreamName);
            return cmi.ConnectionManager;
        }

        /// <summary>
        /// Add a connection to project with a specified stream name
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="streamname"></param>
        /// <returns>connection stream name</returns>
        public ConnectionManager AddConnectionManager(ConnectionManager connection, string streamname)
        {
            Debug.Assert(null != connection, @"The connection should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(streamname), @"The stream name should not be nullable.");

            ConnectionManagerItem cmi = this.Project.ConnectionManagerItems.Add(connection.CreationName, streamname);
            return cmi.ConnectionManager;
        }

        

        /// <summary>
        /// Remove a connection at specified index position
        /// </summary>
        /// <param name="index">the index to specify the connection in project</param>
        public void RemoveConnectionManagerAt(int index)
        {
            Debug.Assert(index > -1, @"The connection index must be greater than or equal to 0.");

            this.Project.ConnectionManagerItems.RemoveAt(index);
        }

        /// <summary>
        /// Remove a connection by specifiying stream name
        /// </summary>
        /// <param name="packagename">connection stream name</param>
        public void RemoveConnectionManager(string connectionname)
        {
            Debug.Assert(!string.IsNullOrEmpty(connectionname), @"The connection name should not be nullable.");

            this.Project.ConnectionManagerItems.Remove(connectionname);
        }

        /// <summary>
        /// Add a parameter for a project.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type</param>
        public Parameter AddProjectParameter(string name, TypeCode type)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), @"The parameter name should not be nullable.");

            return this.Parameters.Add(name, type);
        }

        /// <summary>
        /// Add parameter for a project
        /// </summary>
        /// <param name="name">parameter's name</param>
        /// <param name="type">parameter's type</param>
        /// <param name="required">parameter is required</param>
        /// <param name="sensitive">parameter is sensitive</param>
        /// <param name="defaultValue">parameter's default value</param>
        /// <param name="value">parameter's value</param>
        /// <returns>parameter object</returns>
        public Parameter AddProjectParameter(string name, TypeCode type, bool required, bool sensitive, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), @"The parameter name should not be nullable.");

            Parameter param = this.Parameters.Add(name, type);
            param.Required = required;
            param.Sensitive = sensitive;
            param.Value = value;
            return param;
        }

        /// <summary>
        /// Add package parameter for a project
        /// </summary>
        /// <param name="name">parameter's name</param>
        /// <param name="type">parameter's type</param>
        /// <param name="pkgStreamName">package stream name</param>
        /// <returns>parameter object</returns>
        public Parameter AddPackageParameter(string name, TypeCode type, string pkgStreamName)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), @"The parameter name should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(pkgStreamName), @"The package stream name should not be nullable.");

            //if pacakge exists, add the parameter, else throw exception
            if (this.PackageItems.IndexOf(pkgStreamName) > -1)
            {
                Parameter param = this.PackageItems[pkgStreamName].Package.Parameters.Add(name, type);
                return param;
            }
            else
            {
                throw new ApplicationException("The specified package doesn't exist in the project");
            }
        }

        /// <summary>
        /// Add pacakge parameter for a project
        /// </summary>
        /// <param name="name">parameter's name</param>
        /// <param name="type">parameter's type</param>
        /// <param name="required">parameter is required</param>
        /// <param name="sensitive">parameter is sensitive</param>
        /// <param name="defaultValue">parameter's default value</param>
        /// <param name="value">parameter's value</param>
        /// <param name="pkgStreamName"></param>
        /// <returns>parameter object</returns>
        public Parameter AddPackageParameter(string name, TypeCode type, bool required, bool sensitive, object value, string pkgStreamName)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), @"The parameter name should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(pkgStreamName), @"The package stream name should not be nullable.");

            //if pacakge exists, add the parameter, else throw exception
            if (this.PackageItems.IndexOf(pkgStreamName) > -1)
            {
                Parameter param = this.PackageItems[pkgStreamName].Package.Parameters.Add(name, type);
                param.Required = required;
                param.Sensitive = sensitive;
                param.Value = value;
                return param;
            }
            else
            {
                throw new ApplicationException("The specified package doesn't exist in the project");
            }
        }

        /// <summary>
        /// Remove a parameter from project
        /// </summary>
        /// <param name="name">parameter name</param>
        public void RemoveParameter(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), @"The parameter name should not be nullable.");

            this.Parameters.Remove(name);
        }

        /// <summary>
        /// Remove a parameter from project by index
        /// </summary>
        /// <param name="index">parameter index</param>
        public void RemoveParameter(int index)
        {
            Debug.Assert(index > -1, @"The parameter index must be greater than or equal to 0.");

            this.Parameters.RemoveAt(index);
        }

        /// <summary>
        /// Open project by specifying a file path
        /// </summary>
        /// <param name="filename">the project file path</param>
        public void OpenProject(string filename)
        {
            Debug.Assert(File.Exists(filename), @"The specified file path does not exist.");

            this.m_project = Project.OpenProject(filename);
        }

        /// <summary>
        /// Release all resource used by current instance of project
        /// </summary>
        public void CloseProject()
        {
            m_project.Dispose();
        }

        /// <summary>
        /// Open project by specifying stream
        /// </summary>
        /// <param name="stream">project stream</param>
        public void OpenProject(Stream stream)
        {
            Debug.Assert(Stream.Null != stream, @"The specified stream should not be nullable.");

            this.m_project = Project.OpenProject(stream);
        }

        /// <summary>
        /// Save a project to a file.
        /// </summary>
        /// <param name="filename">target file path</param>
        public void SaveTo(string filename)
        {
            Debug.Assert(!string.IsNullOrEmpty(filename), @"The specified file path should not be nullable.");
            this.Project.SaveTo(filename);
        }

        /// <summary>
        /// Save a project to a stream.
        /// </summary>
        /// <param name="stream">target stream</param>
        public void SaveTo(Stream stream)
        {
            Debug.Assert(Stream.Null != stream, @"The specified stream should not be nullable.");

            this.Project.SaveTo(stream);
        }

        /// <summary>
        /// Save a project as a file.
        /// </summary>
        /// <param name="filename">target file path</param>
        public void SaveAs(string filename)
        {
            Debug.Assert(!string.IsNullOrEmpty(filename), @"The specified file path should not be nullable.");

            this.Project.SaveAs(filename);
        }

        /// <summary>
        /// Save a project as a stream.
        /// </summary>
        /// <param name="stream">target stream</param>
        public void SaveAs(Stream stream)
        {
            Debug.Assert(Stream.Null != stream, @"The specified stream should not be nullable.");

            this.Project.SaveAs(stream);
        }

        /// <summary>
        /// Save the project
        /// </summary>
        public void Save()
        {
            this.Project.Save();
        }
        #endregion
    }

    /// <summary>
    /// EzSourceDestinationProject is a project which contains a data flow package.
    /// In the data flow package, the data flow only contains source and destination.
    /// It will be inherited by specific project containing a data flow package.
    /// </summary>
    public class EzSourceDestinationProject : EzProject
    {
        #region Constructors & operator override
        public EzSourceDestinationProject()
            : base()
        { }

        public EzSourceDestinationProject(string projectfile)
            : base(projectfile)
        { }

        public EzSourceDestinationProject(Project project)
            : base(project)
        { }

        /// <summary>
        /// Override operator, in order to return an EzSourceDestinationProject from an project
        /// </summary>
        /// <param name="project">operand of Project object</param>
        /// <returns>Return a EzSourceDestinationProject object</returns>
        public static implicit operator EzSourceDestinationProject(Project project)
        {
            return new EzSourceDestinationProject(project);
        }

        /// <summary>
        /// Override operator, in order to return a Project from an EzSourceDestinationProject
        /// </summary>
        /// <param name="project">operand of EzSourceDestinationProject object</param>
        /// <returns>Return a Project object</returns>
        public static implicit operator Project(EzSourceDestinationProject project)
        {
            return project.Project;
        }
        #endregion

        /// <summary>
        /// Add an source destionation package in project
        /// </summary>
        /// <typeparam name="SourceType">Source Adapter Type</typeparam>
        /// <typeparam name="SourceConnectionType">Source Connection Manager Type</typeparam>
        /// <typeparam name="TranformType">Tranform Component Type</typeparam>
        /// <typeparam name="DestinationType">Destination Adapter Type</typeparam>
        /// <typeparam name="DestinationConnectionType">Destination Connection Manager Type</typeparam>
        /// <param name="source">source adapter</param>
        /// <param name="srcconn">source connection manager</param>
        /// <param name="destination">destination adapter</param>
        /// <param name="destconn">destination connection manager</param>
        /// <returns>package stream name</returns>
        public string AddPackage<SourceType, SourceConnectionType, DestinationType, DestinationConnectionType>
                (SourceType source,
                SourceConnectionType srcconn,
                DestinationType destination,
                DestinationConnectionType destconn)
            where SourceType : EzAdapter
            where SourceConnectionType : EzConnectionManager
            where DestinationType : EzAdapter
            where DestinationConnectionType : EzConnectionManager
        {
            Debug.Assert(null != source, @"The source component could not be nullable.");
            Debug.Assert(null != srcconn, @"The source connection could not be nullable.");
            Debug.Assert(null != destination, @"The destination component could not be nullable.");
            Debug.Assert(null != destconn, @"The destination connection could not be nullable.");

            //construct a srouce destionation package
            EzSrcDestPackage<SourceType, SourceConnectionType, DestinationType, DestinationConnectionType> package =
                    new EzSrcDestPackage<SourceType, SourceConnectionType, DestinationType, DestinationConnectionType>();
            package.Source = source;
            package.SrcConn = srcconn;
            package.Dest = destination;
            package.DestConn = destconn;

            //Add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an Ado.Net package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="server">source/destination server name</param>
        /// <param name="database">source/destination database name</param>
        /// <param name="srcsql">source sql command statement</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddAdoNetPackage(string server, string database, string srcsql, string desttable)
        {
            Debug.Assert(!string.IsNullOrEmpty(server), @"The server should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(database), @"The database should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table should not be nullable.");

            return AddAdoNetPackage(server, database, srcsql, server, database, desttable);
        }

        /// <summary>
        /// Add an Ado.Net package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddAdoNetPackage(string srcserver, string srcdb, string srcsql,
                string destserver, string destdb, string desttable)
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            //construct a package
            EzSrcDestPackage<EzAdoNetSource, EzSqlAdoNetCM, EzAdoNetDestination, EzSqlAdoNetCM> package =
                    new EzSrcDestPackage<EzAdoNetSource, EzSqlAdoNetCM, EzAdoNetDestination, EzSqlAdoNetCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            //add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an Oledb source and destination package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="server">source/destination server name</param>
        /// <param name="database">source/destination database name</param>
        /// <param name="srcsql">source sql command statement</param>
        /// <param name="transform">tranform component</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddOleDbPackage<TransformType>(string server, string database, string srcsql, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(server), @"The server should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(database), @"The database should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table should not be nullable.");

            return AddOleDbPackage(server, database, srcsql, server, database, desttable);
        }

        /// <summary>
        /// Add an Oledb Source and destiantion package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddOleDbPackage(string srcserver, string srcdb, string srcsql,
                string destserver, string destdb, string desttable)
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            //construct a transform package
            EzSrcDestPackage<EzOleDbSource, EzSqlOleDbCM, EzOleDbDestination, EzSqlOleDbCM> package =
                    new EzSrcDestPackage<EzOleDbSource, EzSqlOleDbCM, EzOleDbDestination, EzSqlOleDbCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            //add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an oledb source and flat file destination package in project
        /// </summary>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="file">destination flat file</param>
        /// <returns>package stream name</returns>
        public string AddOleDbToFilePackage(string srcserver, string srcdb, string srcsql, string destfile)
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destfile), @"The destination file could not be nullable.");

            EzSrcDestPackage<EzOleDbSource, EzSqlOleDbCM, EzFlatFileDestination, EzFlatFileCM> package =
                    new EzSrcDestPackage<EzOleDbSource, EzSqlOleDbCM, EzFlatFileDestination, EzFlatFileCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.DestConn.ConnectionString = destfile;
            package.Dest.Overwrite = true;
            package.Dest.DefineColumnsInCM();

            return this.AddPackage(package);
        }

        /// <summary>
        /// Add a flat file source and oledb destination package in project
        /// </summary>
        /// <param name="srcfile">source flat file</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddFlatFileToOleDbPackage<TransformType>(string srcfile, string destserver, string destdb, string desttable)
        {
            Debug.Assert(!string.IsNullOrEmpty(srcfile), @"The source file could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            EzSrcDestPackage<EzFlatFileSource, EzFlatFileCM, EzOleDbDestination, EzSqlOleDbCM> package =
                    new EzSrcDestPackage<EzFlatFileSource, EzFlatFileCM, EzOleDbDestination, EzSqlOleDbCM>();
            package.SrcConn.ConnectionString = srcfile;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            return this.AddPackage(package);
        }
    }

    /// <summary>
    /// EzTransformProject is a project type which contains a transform package, 
    /// it provides the overrided methods to add transform pacakges.
    /// </summary>
    public class EzTransformProject : EzProject
    {
        #region constructor & operator override
        public EzTransformProject()
            : base()
        { }

        public EzTransformProject(string projectfile)
            : base(projectfile)
        { }

        public EzTransformProject(Project project) : base(project) { }

        /// <summary>
        /// Override operator, in order to return an EzTransformProject from an project
        /// </summary>
        /// <param name="project">operand of Project object</param>
        /// <returns>Return a EzTransformProject object</returns>
        public static implicit operator EzTransformProject(Project project)
        {
            return new EzTransformProject(project);
        }

        /// <summary>
        /// Override operator, in order to return a Project from an EzTransformProject
        /// </summary>
        /// <param name="project">operand of EzTransformProject object</param>
        /// <returns>Return a Project object</returns>
        public static implicit operator Project(EzTransformProject project)
        {
            return project.Project;
        }
        #endregion

        /// <summary>
        /// Add an tranform package in project
        /// </summary>
        /// <typeparam name="SourceType">Source Adapter Type</typeparam>
        /// <typeparam name="SourceConnectionType">Source Connection Manager Type</typeparam>
        /// <typeparam name="TranformType">Tranform Component Type</typeparam>
        /// <typeparam name="DestinationType">Destination Adapter Type</typeparam>
        /// <typeparam name="DestinationConnectionType">Destination Connection Manager Type</typeparam>
        /// <param name="source">source adapter</param>
        /// <param name="srcconn">source connection manager</param>
        /// <param name="transform">tranform component</param>
        /// <param name="destination">destination adapter</param>
        /// <param name="destconn">destination connection manager</param>
        /// <returns>package stream name</returns>
        public string AddPackage<SourceType, SourceConnectionType, TranformType, DestinationType, DestinationConnectionType>
                (SourceType source,
                SourceConnectionType srcconn,
                TranformType transform,
                DestinationType destination,
                DestinationConnectionType destconn)
            where SourceType : EzAdapter
            where SourceConnectionType : EzConnectionManager
            where TranformType : EzComponent
            where DestinationType : EzAdapter
            where DestinationConnectionType : EzConnectionManager
        {
            Debug.Assert(null != source, @"The source component could not be nullable.");
            Debug.Assert(null != srcconn, @"The source connection could not be nullable.");
            Debug.Assert(null != transform, @"The transform component could not be nullable.");
            Debug.Assert(null != destination, @"The destination component could not be nullable.");
            Debug.Assert(null != destconn, @"The destination connection could not be nullable.");

            //construct a transform package
            EzTransformPackage<SourceType, SourceConnectionType, TranformType, DestinationType, DestinationConnectionType> package =
                    new EzTransformPackage<SourceType, SourceConnectionType, TranformType, DestinationType, DestinationConnectionType>();
            package.Source = source;
            package.SrcConn = srcconn;
            package.Transform = transform;
            package.Dest = destination;
            package.DestConn = destconn;

            //Add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an Ado.Net package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="server">source/destination server name</param>
        /// <param name="database">source/destination database name</param>
        /// <param name="srcsql">source sql command statement</param>
        /// <param name="transform">tranform component</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddAdoNetPackage<TransformType>(string server, string database, string srcsql,
                TransformType transform, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(server), @"The server should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(database), @"The database should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement should not be nullable.");
            Debug.Assert(null != transform, @"The transform component should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table should not be nullable.");

            return AddAdoNetPackage<TransformType>(server, database, srcsql, transform, server, database, desttable);
        }

        /// <summary>
        /// Add an Ado.Net package in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="transform">transform component</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddAdoNetPackage<TransformType>(string srcserver, string srcdb, string srcsql,
                TransformType transform, string destserver, string destdb, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(null != transform, @"The transform component could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            //construct a transform package
            EzTransformPackage<EzAdoNetSource, EzSqlAdoNetCM, TransformType, EzAdoNetDestination, EzSqlAdoNetCM> package =
                    new EzTransformPackage<EzAdoNetSource, EzSqlAdoNetCM, TransformType, EzAdoNetDestination, EzSqlAdoNetCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.Transform = transform;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            //add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an Oledb source and destination package with transform in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="server">source/destination server name</param>
        /// <param name="database">source/destination database name</param>
        /// <param name="srcsql">source sql command statement</param>
        /// <param name="transform">tranform component</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddOleDbPackage<TransformType>(string server, string database, string srcsql,
                TransformType transform, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(server), @"The server should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(database), @"The database should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement should not be nullable.");
            Debug.Assert(null != transform, @"The transform component should not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table should not be nullable.");

            return AddOleDbPackage<TransformType>(server, database, srcsql, transform, server, database, desttable);
        }

        /// <summary>
        /// Add an Oledb Source and destiantion package with transform in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="transform">transform component</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddOleDbPackage<TransformType>(string srcserver, string srcdb, string srcsql,
                TransformType transform, string destserver, string destdb, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(null != transform, @"The transform component could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            //construct a transform package
            EzTransformPackage<EzOleDbSource, EzSqlOleDbCM, TransformType, EzOleDbDestination, EzSqlOleDbCM> package =
                    new EzTransformPackage<EzOleDbSource, EzSqlOleDbCM, TransformType, EzOleDbDestination, EzSqlOleDbCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.Transform = transform;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            //add this package in project
            return this.AddPackage(package);
        }

        /// <summary>
        /// Add an oledb source and flat file destination package with transform in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcserver">source server name</param>
        /// <param name="srcdb">source database name</param>
        /// <param name="srcsql">source sql command statment</param>
        /// <param name="transform">transform component</param>
        /// <param name="file">destination flat file</param>
        /// <returns>package stream name</returns>
        public string AddOleDbToFilePackage<TransformType>(string srcserver, string srcdb, string srcsql,
                TransformType transform, string destfile)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(srcserver), @"The source server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcdb), @"The source database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(srcsql), @"The source sql command statement could not be nullable.");
            Debug.Assert(null != transform, @"The transform component could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destfile), @"The destination file could not be nullable.");

            //construct a transform pacakge
            EzTransformPackage<EzOleDbSource, EzSqlOleDbCM, TransformType, EzFlatFileDestination, EzFlatFileCM> package =
                    new EzTransformPackage<EzOleDbSource, EzSqlOleDbCM, TransformType, EzFlatFileDestination, EzFlatFileCM>();
            package.SrcConn.SetConnectionString(srcserver, srcdb);
            package.Source.SqlCommand = srcsql;
            package.Transform = transform;
            package.DestConn.ConnectionString = destfile;
            package.Dest.Overwrite = true;
            package.Dest.DefineColumnsInCM();

            return this.AddPackage(package);
        }

        /// <summary>
        /// Add a flat file source and oledb destination package with tranform in project
        /// </summary>
        /// <typeparam name="TransformType">Transform Component Type</typeparam>
        /// <param name="srcfile">source flat file</param>
        /// <param name="transform">transform component</param>
        /// <param name="destserver">destination server name</param>
        /// <param name="destdb">destination database name</param>
        /// <param name="desttable">destination table name</param>
        /// <returns>package stream name</returns>
        public string AddFlatFileToOleDbPackage<TransformType>(string srcfile, TransformType transform,
                string destserver, string destdb, string desttable)
                where TransformType : EzComponent
        {
            Debug.Assert(!string.IsNullOrEmpty(srcfile), @"The source file could not be nullable.");
            Debug.Assert(null != transform, @"The transform component could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destserver), @"The destination server could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(destdb), @"The destination database could not be nullable.");
            Debug.Assert(!string.IsNullOrEmpty(desttable), @"The destination table could not be nullable.");

            //construct a transform pacakge
            EzTransformPackage<EzFlatFileSource, EzFlatFileCM, TransformType, EzOleDbDestination, EzSqlOleDbCM> package =
                    new EzTransformPackage<EzFlatFileSource, EzFlatFileCM, TransformType, EzOleDbDestination, EzSqlOleDbCM>();
            package.SrcConn.ConnectionString = srcfile;
            package.Transform = transform;
            package.DestConn.SetConnectionString(destserver, destdb);
            package.Dest.Table = desttable;

            return this.AddPackage(package);
        }
    }

    /// <summary>
    /// EzEPTProject is a project which contains EPT packages,
    /// it provides the overrided methods to add EPT packages.
    /// </summary>
    public class EzEPTProject : EzProject
    {
        #region Constructors & operator override
        public EzEPTProject()
            : base()
        { }

        public EzEPTProject(string projectfile)
            : base(projectfile)
        { }

        public EzEPTProject(Project project)
            : base(project)
        { }

        /// <summary>
        /// Override operator, in order to return an EzEPTProject from an project
        /// </summary>
        /// <param name="project">operand of Project object</param>
        /// <returns>Return a EzEPTProject object</returns>
        public static implicit operator EzEPTProject(Project project)
        {
            return new EzEPTProject(project);
        }

        /// <summary>
        /// Override operator, in order to return a Project from an EzEPTProject
        /// </summary>
        /// <param name="project">operand of EzEPTProject object</param>
        /// <returns>Return a Project object</returns>
        public static implicit operator Project(EzEPTProject project)
        {
            return project.Project;
        }

        /// <summary>
        /// Add EPT package in project.
        /// </summary>
        /// <param name="childPackagePath">child package file path</param>
        /// <param name="execOutOfProc">execute the package out of process</param>
        /// <returns>package stream name</returns>
        public string AddEPTPackage(string childPackagePath, bool execOutOfProc)
        {
            EzExecPackage package = new EzExecPackage(new EzPackage() as EzContainer);
            EzFileCM pkgCM = new EzFileCM(package.Package);
            pkgCM.ConnectionString = childPackagePath;
            package.Connection = pkgCM;
            package.ExecOutOfProcess = execOutOfProc;

            return this.AddPackage(package.Package);
        }
        #endregion
    }
}