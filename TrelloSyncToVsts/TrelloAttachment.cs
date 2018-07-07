using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloSyncToVsts
{
    class TrelloAttachment
    {
        public string Id { get; set; }
        //public int Bytes { get; set; }
        //public DateTime Date { get; set; }
        //public string EdgeColor { get; set; }
        //public string IdMember { get; set; }
        //public bool IsUpload { get; set; }
        //public object MimeType { get; set; }
        public string Name { get; set; }
        public List<TrelloPreview> Previews { get; set; }
        public string Url { get; set; }
        //public int Pos { get; set; }
    }

    public class TrelloPreview
    {
        //public int Bytes { get; set; }
        //public string Url { get; set; }
        //public int Height { get; set; }
        //public int Width { get; set; }
        public string _id { get; set; }
        //public bool Scaled { get; set; }
    }
}
