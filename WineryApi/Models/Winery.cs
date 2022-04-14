using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineryApi.Models
{
    public class Winery
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        //if the name was different on the json for example winery_name can use [BsonElement("winery_name")]
        public string Name { get; set; }
        public string Url { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ImageTitle { get; set; }
        public int Rating { get; set; }
        public Address Address { get; set; }
    }
}
