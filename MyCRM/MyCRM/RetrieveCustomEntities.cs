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
            clientCredentials.UserName.UserName = "your user name";
            clientCredentials.UserName.Password = "Your Password";

            using (serviceProxy = new OrganizationServiceProxy(new Uri("url"), null, clientCredentials, null))
            {
                serviceProxy.EnableProxyTypes();
                var query = new QueryExpression("entity local name");
                query.ColumnSet = new ColumnSet();
                query.ColumnSet.AddColumn("condition");
                var retrievedValue = serviceProxy.RetrieveMultiple(query);
            }
        }
    }
}
