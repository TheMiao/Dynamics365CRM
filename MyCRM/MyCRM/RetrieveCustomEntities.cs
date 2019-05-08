using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace MyCRM
{
    public class RetrieveCustomEntities
    {
        public OrganizationServiceProxy serviceProxy { get; set; }

        public void RetrieveustomEntity()
        {
            var clientCredentials = new ClientCredentials();
            // input the user name
            clientCredentials.UserName.UserName = "your user name";
            // input the pass word
            clientCredentials.UserName.Password = "Your Password";

            // input your instance svc url
            using (serviceProxy = new OrganizationServiceProxy(new Uri("url"), null, clientCredentials, null))
            {
                serviceProxy.EnableProxyTypes();
                // input the logical name of the entity
                var query = new QueryExpression("entity logical name");
                query.ColumnSet = new ColumnSet();
                query.ColumnSet.AddColumn("condition");
                var retrievedValue = serviceProxy.RetrieveMultiple(query);
            }
        }
    }
}
