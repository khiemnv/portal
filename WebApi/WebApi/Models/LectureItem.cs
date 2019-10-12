using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class LectureItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Topic { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public string Link { get; set; }
    }
}
