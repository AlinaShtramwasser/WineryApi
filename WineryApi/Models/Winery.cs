using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineryApi.Models
{
    public class Winery
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ImageTitle { get; set; }
        public int Rating { get; set; }
        public Address Address { get; set; }
    }
}
