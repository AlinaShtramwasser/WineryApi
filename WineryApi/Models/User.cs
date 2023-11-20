using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineryApi.Models
{
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        //Google validates the user and then the user gets added to the db - not saving password - but can register through postman
        //[Required]
        public byte[] PasswordSalt { get; set; }
        //[Required]
        public byte[] PasswordHash {get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Winery[] Wineries { get; set; }
        public Address UserAddress { get; set; }
    }
}
