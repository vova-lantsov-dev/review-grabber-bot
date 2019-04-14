using System.Runtime.Serialization;

namespace ReviewGrabberBot.Models
{
    [DataContract]
    internal sealed class GoogleCommentModel
    {
        [DataMember(Name = "comment")]
        public string Comment { get; set; }
    }
}