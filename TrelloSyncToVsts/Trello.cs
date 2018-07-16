using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

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
            var url = "1/members/me/cards";

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
            var url = $"1/lists/{id}/cards";

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
            var url = $"1/cards/{id}/attachments";

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
            var api = $"{url}?key={trelloKey}&token={trelloToken}";

            var response = client.GetAsync(api).Result;

            return response.Content.ReadAsStringAsync().Result;
        }

        public string MoveCardToList(string idCard, string idList)
        {
            var url = $"1/cards/{idCard}/idList";

            var param = $"value={idList}";

            var result = Put(url, param);

            return result;
        }

        private string Put(string url, string param)
        {
            var api = $"{url}?key={trelloKey}&token={trelloToken}&{param}";

            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = client.PutAsync(api, content).Result;

            return response.Content.ReadAsStringAsync().Result;
        }

        public string AddCommentToCard(string id, string comment)
        {
            var url = $"1/cards/{id}/actions/comments";

            var param = $"text={comment}";

            var result = Post(url, param);

            return result;
        }

        private string Post(string url, string param)
        {
            var api = $"{url}?key={trelloKey}&token={trelloToken}&{param}";

            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = client.PostAsync(api, content).Result;

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
