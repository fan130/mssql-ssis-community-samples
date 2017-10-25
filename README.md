**Project Description**  
Microsoft SQL Server Integration Services (SSIS) makes it possible to build high performance data integration solutions, including the extraction, transformation, and loading (ETL) of data for data warehousing and as well as Data Transformation Services (DTS) backward compatible support.  
  
This project contains Integration Services samples created by the product team. If you have questions or suggestions about any of these samples or Integration Services, please let us know by posting in the [SSIS forum](http://forums.microsoft.com/MSDN/ShowForum.aspx?ForumID=80&SiteID=1).
  
For recent Integration Services downloads, samples, articles, videos and more from Microsoft and the community, click [here](http://msdn.microsoft.com/en-us/sqlserver/cc511477.aspx).
# Release Information  
Each sample has its own release on the site. Each release contains an MSI that make it easy to install and get working. In most cases, only the binary is provided by the MSI. To view the source code, click the Source Code tab above. 
  
Unless otherwise noted, all samples are built for SQL Server 2008. 
## [XML Destination Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17643)  
This sample includes source and binary for a simple XML Destination pipeline component.  Use this sample to learn more about how to:
* Create custom data flow destination components for use with SSIS  
* Build component user interfaces
* Support multiple inputs on a single component
  
Written by David Noor
  
(Update to support SQL Server 2012 added)
## [Regular Expression Flat File Source Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17644)  
This sample includes source and binary for a regular expression based flat file parsing source. Use this sample to learn more about how to:  
* Create custom data flow source components for use with SSIS  
* Support multiple outputs from a single component  
* Define output columns  
* Validate metadata
  
Written by Silviu Guea  
  
(Update to support SQL Server 2012 added)
## [Delimited File Reader Source Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17646)  
This sample includes the binaries for a source component capable of parsing delimited flat files, including files with rows that are missing column fields. Use this sample to learn more about how to:  
* Create custom data flow source components for use with SSIS  
* Use Visual Studio Test Edition-based unit tests to perfom automated unit testing on data flow components  
* Respond to and perform custom UI interaction when a component is dragged to the design surface  
* Redirect error rows to an error output  
  
Written by Bob Bojanic  
  
(Update to support SQL Server 2012 added)
## [Package Generation Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17647)  
This sample demonstrates package generation and execution using the Integration Services object model. The sample can be used to transfer data between a pair of source and destination components at the command line. The sample supports three types of source and destination components: SQL Server, Excel and flat file. Use this sample to learn more about how to:  
* Create packages using the SSIS API  
* Validate packages using the SSIS API  
* Execute packages using the SSIS API  
* Add type conversions using the SSIS API  
  
Written by Jia Li  
## [Hello World Task Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17648)  
This sample shows how to create a simple task that extends the SSIS base UI classes to provide a custom UI with the same look and feel as the SSIS tasks that ship with SQL Server 2008. It also shows:  
* Custom UI: The task extends the DTSBaseTaskUI class to have a simple UI with the same look and feel as other SSIS tasks.  
  
Written by Matt Masson  
  
(Update to support SQL Server 2012 added)  
## [SharePoint List Source and Destination Sample](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17652)  
**[MSDN Tutorial](http://msdn.microsoft.com/en-us/library/hh368261.aspx)**  
This sample demonstrates how to get data into and out of SharePoint lists by using custom source and destination adapters written in C# 3.0.  Use this sample to learn more about:   
* Extensive Validation: Validation for this component actively goes against the SharePoint Site to verify the properties are valid.  
* Values from Expressions: This component supports external variables, which can be expressions, and can be attached to the source component to customize the query. Similar to the CommandText for the other Sql Components  
* Linq: The Component has been written using Linq with .net 3.5 and shows how elements such as the metadata and columns can be combined to create a readable usage in a Linq format.  
* Custom properties: The component keeps its configuration in custom properties on itself, inputs, and input columns.  
* FAQ: [http://sqlsrvintegrationsrv.codeplex.com/wikipage?title=SharePoint%20List%20Adapters&referringTitle=Documentation](http://sqlsrvintegrationsrv.codeplex.com/wikipage?title=SharePoint%20List%20Adapters&referringTitle=Documentation)  
  
Written by Kevin Idzi  
## [WMI Source Component Samples](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17655)  
This sample demonstrates how to use WMI (Windows Management Instrumentation) as a data source. Using WMI, you can retrieve and process all kinds of data about your system's performance using SSIS data flows. Use this sample to learn more about:
* Data type mapping: Correctly mapping data between custom types and SSIS types  
* Managing external metadata* Interacting with WMI  
  
Written by Alexander Vikhorev  
  
(Update to support SQL Server 2012 added)  
## [Spatial Data Flow Components](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=17835)  
This sample contains three components showing how to use spatial data within synchronous and asynchronous data flow transforms:
* **Spatial Grid**: replicating rows with SqlGeometry data by cutting geometry object in pieces on a given grid  
* **Spatial Union**: aggregation of spatial data grouped by a regular column. The sample is simplified by requiring a sorted group by column.  
* **Vector Transformations**: applying series of transformations (translations, rotations and scaling), defined using simple expressions, on geometry objects contained in a SqlGeometry column.  
  
Written by Bob Bojanic  
## [MERGE Destination](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=19048)  
This sample contains a custom component showing how to combine ADO.NET's new table-valued parameter support, the new SQL MERGE statement, and SSIS together to create a powerful and fast component for performing MERGE operations against SQL Server. Use this sample to learn more about:  
* Writing custom data flow components* Error redirection  
* Using new ADO.NET TVP support  
  
Written by Sourav Mukherjee and Terri Chen  
  
(Update to support SQL Server 2012 added)
## [BizTalk to Integration Services Samples](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=19347)  
BizTalk Server is designed to help users easily integrate various applications and systems to talk to each other. These samples demonstrate how to integrate SQL Server Integration Services 2005 with BizTalk 2006. They show you how to:  
* Invoke an SSIS package from a BizTalk orchestration using an expression shape  
* Invoke an SSIS package from a BizTalk pipeline using a custom pipeline component  
* Pass information from BizTalk to SSIS  
  
It is recommended that you read the BizTalkIntegationSamplesReadme.rtf document below before installing the sample.  
  
Written by Cho Yeung  
## [EzAPI - Package Generation API](http://www.codeplex.com/SQLSrvIntegrationSrv/Release/ProjectReleases.aspx?ReleaseId=21238)  
This sample provides some functionality to easily create SSIS packages programmatically and dynamically alter their objects (tasks, components, changing metadata, connection strings, etc). This framework supports:  
* Creation SSIS packages of any complexity including both SSIS runtime and pipeline (tasks, containers and components)  
* BIDS like behavior (automatic column mapping in destinations, automatic metadata refresh, default values of properties, etc)  
  
Written by Evgeny Koblov  
  
(Update to support SQL Server 2012 added)
