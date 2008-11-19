using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
//using System.ServiceModel;
using Microsoft.Samples.DataServices.Connectivity.SitkaClient;

[assembly: CLSCompliant(true)]

namespace Microsoft.Samples.DataServices.Connectivity
{
    public class Connection : IDisposable
    {
        const string GetAllQuery = "from e in entities select e";

        private SitkaClient.SitkaSoapEndpoint service = new SitkaClient.SitkaSoapEndpoint();
        private SitkaClient.Scope _scope = new SitkaClient.Scope();

        private Authority authority = null;

        /// <summary>
        /// Construct the object with the static Create() methods
        /// </summary>
        private Connection(string authorityId)
        {
            this.authority = new Authority(authorityId);
            _scope.AuthorityId = authorityId;
        }

        public Authority Authority
        {
            get
            {
                return authority;
            }
        }

        /// <summary>
        /// SSDS service URL
        /// </summary>
        public string Url
        {
            get
            {
                return service.Url;
            }
            set
            {
                service.Url = value;
            }
        }

        public static string GetDefaultServiceUrl()
        {
            SitkaClient.SitkaSoapEndpoint s = new SitkaClient.SitkaSoapEndpoint();
            return s.Url;
        }

        /// <summary>
        /// Creates a new SSDS Connection.
        /// </summary>
        /// <param name="authorityId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Connection Create(string authorityId, string userName, string password)
        {
            Connection newConnection = new Connection(authorityId);
            
            // Set the credentials
            newConnection.service.Credentials = new NetworkCredential(userName, password);

            // Set up our _scope object
            newConnection._scope.AuthorityId = authorityId;

            return newConnection;
        }

        public Container GetContainerById(string id)
        {
            SitkaClient.Scope newScope = new SitkaClient.Scope();
            newScope.AuthorityId = _scope.AuthorityId;
            newScope.ContainerId = id;

            SitkaClient.Entity e = service.Get(newScope);

            Container c = Container.Create(this, e);

            return c;
        }

        public Container[] GetContainers()
        {
            SitkaClient.Entity[] containers = service.Query(_scope, GetAllQuery);

            Container[] newArray = new Container[containers.Length];
            for (int i = 0; i < containers.Length; i++)
            {
                Container c = Container.Create(this, containers[i]);
                newArray[i] = c;
            }

            return newArray;
        }

        /// <summary>
        /// Attempts to establish a connection by sending a query for all containers.
        /// The method returns a bool to indicate whether the query was successful. 
        /// This method does not throw any exceptions, and does not indicate why
        /// a connection or query failed.
        /// To receive the actual connection error, use the TestWithThrow() method.
        /// </summary>
        /// <returns>Returns true if the connection succeeds, false if it fails</returns>
        public bool Test()
        {
            try
            {
                TestWithThrow();
                return true;
            }
            catch (Exception)
            {
                // Ignore the exception - the caller is only interested in the connection status
            }

            return false;
        }

        /// <summary>
        /// Attempts to establish a connection by sending a query for all containers.
        /// The method will throw an exception if the connection fails.
        /// </summary>
        public void TestWithThrow()
        {
            service.Query(_scope, GetAllQuery);
        }

        internal SitkaClient.Scope Scope
        {
            get
            {
                return _scope;
            }
        }

        public Container CreateContainer(String containerId)
        {
            SitkaClient.Container sitkaContainer = new SitkaClient.Container();
            Container container;

            sitkaContainer.Id = containerId;

            service.Create(_scope, sitkaContainer);
            container = Container.Create(this, sitkaContainer);
            return container;
        }

        internal SitkaClient.Scope Create(SitkaClient.Scope scope, SitkaClient.Entity entity)
        {
            return service.Create(scope, entity);
        }

        internal SitkaClient.Entity[] Query(SitkaClient.Scope scope, string query)
        {
            return service.Query(scope, query);
        }

        internal void Delete(SitkaClient.Scope scope)
        {
            service.Delete(scope);
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (service != null)
                {
                    service.Dispose();
                    service = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Connection()
        {
            Dispose(false);
        }

        #endregion
    }
}
