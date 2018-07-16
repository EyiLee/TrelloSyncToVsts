using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TrelloSyncToVsts
{
    class Vsts
    {
        private readonly string vstsUrl;
        private readonly string vstsToken;

        public VssConnection Connection { get; set; }
        public WorkItemTrackingHttpClient WorkItemTrackingClient { get; set; }

        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Constructor of Vsts Class.
        /// </summary>
        /// <param name="vstsUrl">Team url of VSTS.</param>
        /// <param name="vstsToken">Token of VSTS api.</param>
        public Vsts(string vstsUrl, string vstsToken)
        {
            this.vstsUrl = vstsUrl;
            this.vstsToken = vstsToken;

            Connection = new VssConnection(new Uri(vstsUrl), new VssBasicCredential(string.Empty, vstsToken));

            WorkItemTrackingClient = Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public AttachmentReference SyncFile(string fileUrl, string fileName)
        {
            AttachmentReference attachmentReference;

            var response = client.GetAsync(fileUrl);

            var result = response.Result.Content.ReadAsStreamAsync().Result;

            Console.WriteLine($"-- Uploading \"{fileName}\".");

            attachmentReference = WorkItemTrackingClient.CreateAttachmentAsync(result, fileName).Result;

            return attachmentReference;
        }

        public List<AttachmentReference> SyncFiles(List<AttachmentInfo> files)
        {
            List<AttachmentReference> attachmentReferences = new List<AttachmentReference>();

            foreach (var file in files)
            {
                var response = client.GetAsync(file.Url);

                var result = response.Result.Content.ReadAsStreamAsync().Result;

                Console.WriteLine($"-- Uploading \"{file.Name}\".");

                var attachmentReference = WorkItemTrackingClient.CreateAttachmentAsync(result, file.Name).Result;

                attachmentReferences.Add(attachmentReference);
            }

            return attachmentReferences;
        }
    }
}
