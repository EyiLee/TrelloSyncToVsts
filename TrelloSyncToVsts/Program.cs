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

            Console.ReadLine();
        }
    }
}
