using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloSyncToVsts
{
    class TrelloCard
    {
        public string Id { get; set; }
        //public object Badges { get; set; }
        //public object CheckItemStates { get; set; }
        //public bool Closed { get; set; }
        //public DateTime DateLastActivity { get; set; }
        public string Desc { get; set; }
        //public object DescData { get; set; }
        //public DateTime Due { get; set; }
        //public bool DueComplete { get; set; }
        //public string Email { get; set; }
        //public string IdAttachmentCover { get; set; }
        //public string IdBoard { get; set; }
        //public List<string> IdChecklists { get; set; }
        //public List<string> IdLabels { get; set; }
        public string IdList { get; set; }
        public List<string> IdMembers { get; set; }
        //public List<string> IdMembersVoted { get; set; }
        //public int IdShort { get; set; }
        public List<TrelloLabel> Labels { get; set; }
        //public bool ManualCoverAttachment { get; set; }
        public string Name { get; set; }
        //public float Pos { get; set; }
        public string ShortLink { get; set; }
        public string ShortUrl { get; set; }
        //public bool Subscribed { get; set; }
        public string Url { get; set; }
    }
}
