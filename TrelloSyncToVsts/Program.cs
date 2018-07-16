using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrelloSyncToVsts
{
    class Program
    {
        private static readonly string TrelloKey = Properties.Settings.Default.TrelloKey;
        private static readonly string TrelloToken = Properties.Settings.Default.TrelloToken;

        private static readonly string VstsUrl = Properties.Settings.Default.VstsUrl;
        private static readonly string VstsToken = Properties.Settings.Default.VstsToken;

        private static readonly string IdListFrom = Properties.Settings.Default.IdListFrom;
        private static readonly string IdListTo = Properties.Settings.Default.IdListTo;

        private static readonly int IdPbi = Properties.Settings.Default.IdPbi;
        private static readonly string UserName = Properties.Settings.Default.UserName;

        static void Main(string[] args)
        {
            var trello = new Trello(TrelloKey, TrelloToken);

            var vsts = new Vsts(VstsUrl, VstsToken);

            var cards = trello.GetCardsByMe()
                .Where(c => c.IdList == IdListFrom)
                .ToList();

            var targetPBI = vsts.WorkItemTrackingClient.GetWorkItemAsync(IdPbi).Result;

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
                        Value = UserName
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

                var response = vsts.WorkItemTrackingClient.CreateWorkItemAsync(patchDocument, teamProject, "Task").Result;

                trello.MoveCardToList(card.Id, IdListTo);
                trello.AddCommentToCard(card.Id, response.Id.ToString());
                Console.WriteLine("Progress done.\n");
            }

            Console.WriteLine("!! All cards have been synced. !!");
            Console.ReadLine();
        }
    }
}
