/*********
 * Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.Linq;
using System.IO;

namespace SSISMSBuild
{
    public class BuildDeploymentManifest : Task
    {
        [Required]
        public ITaskItem[] InputProject { get; set; }

        // utility method to log the file as we access it.
        // MSBuild handles exceptions fairly well, we won't add anything to the error handling.
        private XDocument LoadAndLog(string filename)
        {
            Log.LogMessage("reading {0}", filename);
            return XDocument.Load(filename);
        }
        private XElement CreateElementForFileAndCopy(string filetype, string sourcepath, string destinationpath)
        {
            Log.LogMessage("copying {0} {1} to {2}", filetype, sourcepath, destinationpath);
            try
            {
                File.Copy(sourcepath, Path.Combine(destinationpath, Path.GetFileName(sourcepath)), true);
            }
            catch (Exception e)
            {
                Log.LogError("Error copying source file: {0}", e.Message);
            }
            return new XElement(filetype, Path.GetFileName(sourcepath));
        }

        public override bool Execute()
        {
            XNamespace dts = "www.microsoft.com/SqlServer/Dts";
            
            // MSBuild itemgroups split multiple files by ;.
            // Each project file becomes a manifest file.
            foreach (ITaskItem file in InputProject)
            {
                Log.LogMessage("----");
                XDocument document = LoadAndLog(file.ItemSpec);
                string outputDirectory = file.GetMetadata("OutputDirectory");
                string projectBase = Path.GetDirectoryName(file.ItemSpec);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    Log.LogError("No output directory specified for {0}", file.ItemSpec);
                    return false;
                }
                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
                // one ginormous LINQ query to build the deployment manifest -- technically one line of code...
                // that is what I get for reading a LINQ book right before this project.
                new XElement("DTSDeploymentManifest",
                    // basic attributes for the manifest, made to look like it came out of SSIS
                    new XAttribute("AllowConfigurationChanges", true),
                    new XAttribute("GeneratedBy", System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()),
                    new XAttribute("GeneratedFromProjectName", Path.GetFileNameWithoutExtension(file.ItemSpec)),
                    new XAttribute("GeneratedDate", System.DateTime.Now),
                    
                    // add all the packages
                    from a in document.Descendants("FullPath")
                    where a.Parent.Name == "DtsPackage"
                    select CreateElementForFileAndCopy("Package", Path.Combine(projectBase, a.Value), outputDirectory),

                    // now, for each of the packages, load them and suck out config file references
                    from a in document.Descendants("FullPath")
                    let packageFileName = Path.Combine(Path.GetDirectoryName(file.ItemSpec), a.Value)
                    where a.Parent.Name == "DtsPackage"
                        from f in LoadAndLog(packageFileName).Descendants(dts + "Property")
                        where f.Parent.Name == dts + "Configuration" &&
                            f.Attribute(dts + "Name") != null &&
                            f.Attribute(dts + "Name").Value == "ConfigurationString" &&
                            f.Parent.Descendants().Any(x => x.Attribute(dts + "Name") != null &&
                                x.Attribute(dts + "Name").Value == "ConfigurationType" && x.Value == "1")
                        select CreateElementForFileAndCopy("ConfigurationFile", Path.Combine(projectBase, f.Value), outputDirectory),
                
                    // miscellaneous files
                    from a in document.Descendants("FullPath")
                    where a.Parent.Name == "ProjectItem" && a.Parent.Parent.Name == "Miscellaneous"
                    select CreateElementForFileAndCopy("MiscellaneousFile", Path.Combine(projectBase, a.Value), outputDirectory)
                    
                // and save it.
                ).Save(Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file.ItemSpec) + ".SSISDeploymentManifest"));
            }
            return true;
        }
    }
}
