using System;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.Samples.DataServices.Connectivity;
using System.Globalization;
using System.ComponentModel;

[assembly:CLSCompliant(true)]

namespace Microsoft.Samples.DataServices
{
    [DtsConnection(
        ConnectionType = "SSDS",
        DisplayName = "Sql Server Data Services",
        Description = "Connection manager for SQL Server Data Services",
        UITypeName = "Microsoft.Samples.DataServices.SsdsConnectionManagerUI, Microsoft.Samples.DataServices.ConnectionManager, Version=1.0.0.0, Culture=neutral,PublicKeyToken=da625e43f8e8d37e"
    )]
    public sealed class SsdsConnectionManager : ConnectionManagerBase, IDTSComponentPersist
    {
        /// <summary>
        /// Holds the values in our connection string. This class will
        /// not be instantiated.
        /// </summary>
        private class ConnectionStringItemList
        {
            public const string Authority = "Authority";
            public const string UserName = "UserName";
            public const string Password = "Password";
            public static string[] Items ={ Authority, UserName, Password };

            private ConnectionStringItemList()
            {
            }
        }       
        
        internal void ParseConnectionString(string connectionString)
        {
            // Get the properties we need in the connection string
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();

            StringBuilder result = new StringBuilder();            
            for (int i = 0; i < ConnectionStringItemList.Items.Length; i++)
            {
                Regex regex = new Regex(String.Format(CultureInfo.InvariantCulture, @"{0}=((.)*?);", ConnectionStringItemList.Items[i]),RegexOptions.IgnoreCase);
                if (regex.IsMatch(connectionString))
                {
                    for (int j = 0; j < propertyInfos.Length; j++)
                    {
                        if (propertyInfos[j].Name.Equals(ConnectionStringItemList.Items[i]))
                        {
                            propertyInfos[j].SetValue(this, regex.Match(connectionString).Groups[1].Value, null);
                        }
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object otherObj)
        {
            return base.Equals(otherObj);
        }
        public override Microsoft.SqlServer.Dts.Runtime.DTSProtectionLevel ProtectionLevel
        {
            get
            {
                return base.ProtectionLevel;
            }
            set
            {
                base.ProtectionLevel = value;
            }
        }
        public override object AcquireConnection(object txn)
        {
            Connection connection = Connection.Create(authority, userName, password);

            // Check if we need to override the URL of the service
            if (!string.IsNullOrEmpty(url))
            {
                connection.Url = url;
            }

            return connection;
        }
        public override void ReleaseConnection(object connection)
        {
            base.ReleaseConnection(connection);
            connection = null;
        }
        

        #region Properties
        private string authority = String.Empty;
        public string Authority
        {
            get
            {
                return authority;
            }
            set
            {
                authority = value;
            }
        }

        private string userName = String.Empty;
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        private string password = String.Empty;
        public string Password
        {
            set
            {
                password = value;
            }
        }
        internal string GetPassword()
        {
            return password;
        }

        public override string ConnectionString
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, 
                                     "{0}={1};{2}={3};", 
                                     ConnectionStringItemList.Authority, Authority, 
                                     ConnectionStringItemList.UserName, userName);
            }
            set
            {
                ParseConnectionString(value);
            }
        }

        private string url = String.Empty;
        
        [Description("Setting a value will override the default SSDS service URI.")]
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }
        #endregion

        #region IDTSComponentPersist Members

        #region String Constants
        private const string PERSIST_XML_ELEMENT = "SSDSConnectionManager";
        private const string PERSIST_XML_CONNECTIONSTRING = "ConnectionString";
        private const string PERSIST_XML_URL = "Url";
        private const string PERSIST_XML_PASSWORD = "PassWord";
        private const string PERSIST_XML_SENSITIVE = "Sensitive";
        #endregion

        void IDTSComponentPersist.LoadFromXML(XmlElement rootNode, IDTSInfoEvents infoEvents)
        {
            // Create an root node for the data
            if (rootNode.Name != PERSIST_XML_ELEMENT)
            {
                throw new ArgumentException("Unexpected element");
            }

            // Unpersist the connection string (excluding the password)
            XmlNode csAttr = rootNode.Attributes.GetNamedItem(PERSIST_XML_CONNECTIONSTRING);
            if (csAttr != null)
            {
                this.ConnectionString = csAttr.Value;
            }

            // Unpersist the password
            // The SSIS runtime will already have decrypted it for us
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                if (childNode.Name == PERSIST_XML_PASSWORD)
                {
                    password = childNode.InnerText;
                }
            }

            // Unpersist the service URL
            XmlNode urlAttr = rootNode.Attributes.GetNamedItem(PERSIST_XML_URL);
            if (urlAttr != null)
            {
                this.url = urlAttr.Value;
            }
        }

        void IDTSComponentPersist.SaveToXML(XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            // Create a root node for the data
            XmlElement rootElement = doc.CreateElement(String.Empty, PERSIST_XML_ELEMENT, String.Empty);
            doc.AppendChild(rootElement);

            // Persist the connection string (excluding the password)
            XmlAttribute csAttr = doc.CreateAttribute(PERSIST_XML_CONNECTIONSTRING);
            csAttr.Value = this.ConnectionString;
            rootElement.Attributes.Append(csAttr);

            // Persist the password separately
            if (!String.IsNullOrEmpty(password))
            {
                XmlNode node = doc.CreateNode(XmlNodeType.Element, PERSIST_XML_PASSWORD, String.Empty);
                XmlElement pswdElement = node as XmlElement;
                rootElement.AppendChild(node);

                // Adding the sensitive attribute tells the SSIS runtime that
                // this value should be protected according to the 
                // ProtectionLevel of the package
                pswdElement.InnerText = password;
                XmlAttribute pwAttr = doc.CreateAttribute(PERSIST_XML_SENSITIVE);
                pwAttr.Value = "1";
                pswdElement.Attributes.Append(pwAttr);
            }

            // Persist the service URL
            XmlAttribute urlAttr = doc.CreateAttribute(PERSIST_XML_URL);
            urlAttr.Value = this.url;
            rootElement.Attributes.Append(urlAttr);
        }

        #endregion
    }
}
