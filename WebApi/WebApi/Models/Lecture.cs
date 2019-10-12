using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApi.Models
{
    public class Lecture
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Topic { get; set; }
        public DateTime Created { get; set; }
        public string Target { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
    }
}
