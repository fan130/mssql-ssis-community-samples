using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.DataServices.Connectivity
{
    public class Entity : BaseEntity
    {
        private string _kind;
        private bool _fail;
        private int _row;

        private Dictionary<string, object> _properties = new Dictionary<string,object>();

        public Entity()
        {
        }

        public Entity(string id)
            : base(id)
        {
        }

        public Entity(string id, int row)
            : base(id)
        {
            _row = row;
        }

        public string Kind
        {
            get
            {
                return this._kind;
            }
            set
            {
                this._kind = value;
            }
        }

        public bool Fail
        {
            get
            {
                return this._fail;
            }
            set
            {
                this._fail = value;
            }
        }

        public int Row
        {
            get
            {
                return this._row;
            }          
        }
        public Dictionary<string, object> Properties
        {
            get
            {
                return this._properties;
            }
        }

        override internal void CopyFrom(SitkaClient.Entity e)
        {
            base.CopyFrom(e);

            this.Kind = e.Kind;

            // Copy properties
            foreach (SitkaClient.ArrayOfKeyValueOfstringanyTypeKeyValueOfstringanyType a in e.Properties)
            {
                this.Properties.Add(a.Key, a.Value);
            }
        }

        override internal void CopyTo(SitkaClient.Entity e)
        {
            base.CopyTo(e);

            e.Kind = this.Kind;

            // Copy properties
            SitkaClient.ArrayOfKeyValueOfstringanyTypeKeyValueOfstringanyType[] array = new SitkaClient.ArrayOfKeyValueOfstringanyTypeKeyValueOfstringanyType[_properties.Count];

            int count = 0;
            foreach (string key in _properties.Keys)
            {
                SitkaClient.ArrayOfKeyValueOfstringanyTypeKeyValueOfstringanyType a = new SitkaClient.ArrayOfKeyValueOfstringanyTypeKeyValueOfstringanyType();

                a.Key = key;
                a.Value = _properties[key];

                array[count++] = a;
            }

            e.Properties = array;
        }
    }
}
