using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            // Initialize connection instances
            Trello trello = new Trello(TrelloKey, TrelloToken);

            Vsts vsts = new Vsts(VstsUrl, VstsToken);

            // Get trello cards list by user
            var cards = trello.GetCardsByMe()
                .Where(c => c.IdList == IdListFrom)
                .ToList();

            // Get VSTS BPI infomation
            var targetPBI = vsts.WorkItemTrackingClient.GetWorkItemAsync(IdPbi).Result;

            var areaPath = targetPBI.Fields["System.AreaPath"].ToString();
            var iterationPath = targetPBI.Fields["System.IterationPath"].ToString();
            var teamProject = targetPBI.Fields["System.TeamProject"].ToString();

            Console.WriteLine("Start syncing trello cards to VSTS.\n");

            foreach (var card in cards)
            {
                Console.WriteLine("Syncing card \"{0}\" to VSTS.", card.Name);

                #region -- Query --

                // Create query for new task of VSTS
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

                #endregion

                #region -- Tags --

                // Extract tags from card name and sync to VSTS
                var matches = Regex.Matches(card.Name, @"(?<=\[)[^\[\]]*(?=\])");

                string tags = string.Empty;
                foreach (var tag in matches)
                {
                    tags += tag + ",";
                }

                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Tags",
                        Value = tags
                    }
                );

                #endregion

                #region -- Attachments --

                // Get attachments from Trello and sync to VSTS
                var attachments = trello.GetAttachmentsByCardId(card.Id);

                var attachmentReferences = vsts.SyncAttachments(attachments);

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
                                    comment = ""
                                }
                            }
                        }
                    );
                }

                #endregion

                var response = vsts.WorkItemTrackingClient.CreateWorkItemAsync(patchDocument, teamProject, "Task").Result;

                trello.MoveCardToList(card.Id, IdListTo);
                trello.AddCommentToCard(card.Id, response.Id.ToString());
                Console.WriteLine("Progress done.\n");
            }

            Console.WriteLine("All cards have been synced.");
            Console.ReadLine();
        }
    }
}
