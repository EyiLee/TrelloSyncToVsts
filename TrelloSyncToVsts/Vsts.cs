using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloSyncToVsts
{
    class Vsts
    {
        private readonly string vstsUrl;
        private readonly string vstsToken;

        public VssConnection Connection { get; set; }

        public Vsts(string vstsUrl, string vstsToken)
        {
            this.vstsUrl = vstsUrl;
            this.vstsToken = vstsToken;

            Connection = new VssConnection(new Uri(vstsUrl), new VssBasicCredential(string.Empty, vstsToken));
        }
    }
}
