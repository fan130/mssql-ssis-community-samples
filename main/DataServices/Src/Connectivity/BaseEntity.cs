using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.DataServices.Connectivity
{
    public abstract class BaseEntity
    {
        private string idField;

        private long versionField = 0;

        private bool versionFieldSpecified = false;

        protected BaseEntity()
        {
            idField = string.Empty;
        }

        protected BaseEntity(string id)
        {
            idField = id;
        }

        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        public long Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
                this.versionFieldSpecified = true;
            }
        }

        public bool VersionSpecified
        {
            get
            {
                return this.versionFieldSpecified;
            }
        }

        internal virtual void CopyFrom(SitkaClient.Entity e)
        {
            this.idField = e.Id;
            this.versionField = e.Version;
            this.versionFieldSpecified = e.VersionSpecified;
        }

        internal virtual void CopyTo(SitkaClient.Entity e)
        {
            e.Id = this.idField;
            e.VersionSpecified = this.versionFieldSpecified;
            e.Version = this.versionField;
        }
    }
}
