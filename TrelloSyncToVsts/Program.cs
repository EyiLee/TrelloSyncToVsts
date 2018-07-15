using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloSyncToVsts
{
    class Program
    {
        private static readonly string trelloKey = "";
        private static readonly string trelloToken = "";

        private static readonly string trelloList = "";

        private static readonly string vstsUrl = "";
        private static readonly string vstsToken = "";

        private static readonly int vstsPBI = 0;
        private static readonly string vstsUser = "";

        static void Main(string[] args)
        {
            var trello = new Trello(trelloKey, trelloToken);

            var vsts = new Vsts(vstsUrl, vstsToken);

            var cards = trello.GetCardsByMe()
                .Where(c => c.IdList == trelloList)
                .ToList();

            var targetPBI = vsts.WorkItemTrackingClient.GetWorkItemAsync(vstsPBI).Result;

            var areaPath = targetPBI.Fields["System.AreaPath"].ToString();
            var iterationPath = targetPBI.Fields["System.IterationPath"].ToString();
            var teamProject = targetPBI.Fields["System.TeamProject"].ToString();

            Console.WriteLine("Start syncing trello cards to VSTS.\n");

            foreach (var card in cards)
            {
                Console.WriteLine("Syncing card \"{0}\" to VSTS.", card.Name);

                var attachments = trello.GetAttachmentsByCardId(card.Id);

                List<AttachmentInfo> attachmentInfos = new List<AttachmentInfo>();
                foreach (var attachment in attachments)
                {
                    AttachmentInfo item = new AttachmentInfo()
                    {
                        Url = attachment.Url,
                        Name = attachment.Name
                    };

                    attachmentInfos.Add(item);
                }

                var attachmentReferences = vsts.SyncFiles(attachmentInfos);

                JsonPatchDocument patchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Title",
                        Value = card.ShortLink + card.Name
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.AssignedTo",
                        Value = vstsUser
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.AreaPath",
                        Value = areaPath
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.IterationPath",
                        Value = iterationPath
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Description",
                        Value = card.Desc
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.History",
                        Value = card.ShortUrl
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork",
                        Value = 0.25
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.Common.Activity",
                        Value = "Development"
                    },

                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/relations/-",
                        Value = new
                        {
                            rel = "System.LinkTypes.Hierarchy-Reverse",
                            url = targetPBI.Url,
                            attributes = new
                            {
                                comment = "Link to parent"
                            }
                        }
                    }
                };

                foreach (var attachmentreference in attachmentReferences)
                {
                    patchDocument.Add(
                        new JsonPatchOperation()
                        {
                            Operation = Operation.Add,
                            Path = "/relations/-",
                            Value = new
                            {
                                rel = "AttachedFile",
                                url = attachmentreference.Url,
                                attributes = new
                                {
                                    comment = "Attach new file"
                                }
                            }
                        }
                    );
                }

                vsts.WorkItemTrackingClient.CreateWorkItemAsync(patchDocument, teamProject, "Task").Wait();
                Console.WriteLine("Progress done.\n");
            }

            Console.WriteLine("!! All cards have been synced. !!");
            Console.ReadLine();
        }
    }
}
