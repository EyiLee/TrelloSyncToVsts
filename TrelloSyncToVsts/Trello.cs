using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public Trello(string trelloKey, string trelloToken)
        {
            this.trelloKey = trelloKey;
            this.trelloToken = trelloToken;
        }

        public List<TrelloCard> GetCardsByListId(string id)
        {
            var url = String.Format("1/lists/{0}/cards", id);

            var result = Get(url);

            return JsonConvert.DeserializeObject<List<TrelloCard>>(result);
        }

        public List<TrelloAttachment> GetAttachmentsByCardId(string id)
        {
            var url = String.Format("1/cards/{0}/attachments", id);

            var result = Get(url);

            return JsonConvert.DeserializeObject<List<TrelloAttachment>>(result);
        }

        private string Get(string url)
        {
            var api = String.Format("{0}?key={1}&token={2}", url, trelloKey, trelloToken);

            var response = client.GetAsync(api).Result;

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
