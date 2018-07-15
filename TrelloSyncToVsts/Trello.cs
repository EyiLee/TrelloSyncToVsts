using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TrelloSyncToVsts
{
    class Trello
    {
        private readonly string trelloKey;
        private readonly string trelloToken;

        private static HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("https://api.trello.com")
        };

        /// <summary>
        /// Constructor of Trello Class.
        /// </summary>
        /// <param name="trelloKey">Key of Trello api.</param>
        /// <param name="trelloToken">Token of Trello api.</param>
        public Trello(string trelloKey, string trelloToken)
        {
            this.trelloKey = trelloKey;
            this.trelloToken = trelloToken;
        }

        /// <summary>
        /// Get cards information by me.
        /// </summary>
        /// <returns>List of cards.</returns>
        public List<TrelloCard> GetCardsByMe()
        {
            var url = String.Format("1/members/me/cards");

            var result = Get(url);

            return JsonConvert.DeserializeObject<List<TrelloCard>>(result);
        }

        /// <summary>
        /// Get cards information by listid.
        /// </summary>
        /// <param name="id">Id of the list.</param>
        /// <returns>List of cards.</returns>
        public List<TrelloCard> GetCardsByListId(string id)
        {
            var url = String.Format("1/lists/{0}/cards", id);

            var result = Get(url);

            return JsonConvert.DeserializeObject<List<TrelloCard>>(result);
        }

        /// <summary>
        /// Get attachments information by cardid
        /// </summary>
        /// <param name="id">Id of the card.</param>
        /// <returns>List of attachments on the card.</returns>
        public List<TrelloAttachment> GetAttachmentsByCardId(string id)
        {
            var url = String.Format("1/cards/{0}/attachments", id);

            var result = Get(url);

            return JsonConvert.DeserializeObject<List<TrelloAttachment>>(result);
        }

        /// <summary>
        /// Base method for Trello api.
        /// </summary>
        /// <param name="url">Route of the target api.</param>
        /// <returns>Response of the target api.</returns>
        private string Get(string url)
        {
            var api = String.Format("{0}?key={1}&token={2}", url, trelloKey, trelloToken);

            var response = client.GetAsync(api).Result;

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
