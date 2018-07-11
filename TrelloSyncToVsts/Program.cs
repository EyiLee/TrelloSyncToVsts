using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloSyncToVsts
{
    class Program
    {
        private static readonly string trelloKey = "";
        private static readonly string trelloToken = "";

        private static readonly string vstsUrl = "";
        private static readonly string vstsToken = "";

        static void Main(string[] args)
        {
            var trello = new Trello(trelloKey, trelloToken);

            var vsts = new Vsts(vstsUrl, vstsToken);

            var workItemTrackingClient = vsts.Connection.GetClient<WorkItemTrackingHttpClient>();

            var targetWorkItem = workItemTrackingClient.GetWorkItemAsync(2).Result;

            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Forward",
                        url = targetWorkItem.Url,
                        attributes = new
                        {
                            comment = "Making a new link for the dependency"
                        }
                    }
                }
            };

            var result = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, 1).Result;
        }
    }
}
