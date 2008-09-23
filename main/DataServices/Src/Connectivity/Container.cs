using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.DataServices.Connectivity
{
    public class Container : BaseEntity
    {
        internal Connection connection = null;

        private Container()
        {
        }

        public Entity[] GetEntities(string kind)
        {
            return GetEntities(kind, string.Empty, 0);
        }

        public Entity[] GetEntities(string kind, string lastId)
        {
            return GetEntities(kind, lastId, 0);
        }

        public Entity[] GetEntities(string kind, string lastId, int max)
        {
            // Setup the query
            bool hasWhere = false;
            string query = string.Empty;
            StringBuilder sb = new StringBuilder("from e in entities ");

            // limit the query by kind
            if (!string.IsNullOrEmpty(kind))
            {
                if (!hasWhere)
                {
                    sb.Append("where ");
                    hasWhere = true;
                }
                else
                {
                    sb.Append("&& ");
                }

                // NOTE: We don't worry about SQL injection type attacks for sitka queries, but
                // we would have to protect against them if this query was being executed
                // against a database
                sb.Append("e.Kind == \"");
                sb.Append(EscapeQuotes(kind));
                sb.Append("\" ");
            }

            if (!string.IsNullOrEmpty(lastId))
            {
                // check if we already have a where clause
                if (!hasWhere)
                {
                    sb.Append("where ");
                }
                else
                {
                    sb.Append("&& ");
                }

                // NOTE: We don't worry about SQL injection type attacks for sitka queries, but
                // we would have to protect against them if this query was being executed
                // against a database
                sb.Append("e.Id > \"");
                sb.Append(EscapeQuotes(lastId));
                sb.Append("\" ");
            }

            sb.Append("select e");

            // TODO: add clause to limit the number of entities we get back
            // The Sitka API doesn't currently support this
            if (max > 0)
            {
            }

            query = sb.ToString();

            // Setup the _scope
            SitkaClient.Scope scope = connection.Scope;
            scope.ContainerId = this.Id;

            SitkaClient.Entity[] sitkaEntities = connection.Query(scope, query);

            int count = sitkaEntities.Length;

            // TODO: Remove once we can limit via query
            if (max > 0)
            {
                // We're limiting the number of results
                count = Math.Min(max, sitkaEntities.Length);
            }

            Entity[] entities = new Entity[count];
            for (int i = 0; i < entities.Length; i++)
            {
                Entity e = new Entity();
                e.CopyFrom(sitkaEntities[i]);

                entities[i] = e;
            }

            return entities;
        }

        /// <summary>
        /// Deletes the Container from the current Authority
        /// </summary>
        public void Delete()
        {
            SitkaClient.Scope scope = connection.Scope;
            scope.ContainerId = this.Id;

            connection.Delete(scope);
        }

        /// <summary>
        /// Inserts a new entity. If the insert fails, an exception will be thrown.
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        public void InsertEntity(Entity entity)
        {
            // convert to Sitka format
            SitkaClient.Entity sitkaEntity = new SitkaClient.Entity();
            entity.CopyTo(sitkaEntity);

            SitkaClient.Scope scope = connection.Scope;
            scope.ContainerId = this.Id;

            connection.Create(scope, sitkaEntity);
        }

        static internal Container Create(Connection connection, SitkaClient.Entity sitkaContainer)
        {
            Container newContainer = new Container();
            newContainer.CopyFrom(sitkaContainer);
            newContainer.connection = connection;

            return newContainer;
        }

        /// <summary>
        /// Preserves quotes in the given string by adding \ to escape them.
        /// This works for sitka queries - this method will not adequately protect
        /// strings used in regular SQL statements from SQL injection attacks
        /// </summary>
        /// <param name="str">string to escape</param>
        /// <returns>escaped string</returns>
        private string EscapeQuotes(string str)
        {
            return str.Replace("\"", "\\\"");
        }
    }
}
