using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PointBot.Models
{
    public class PointEvent
    {
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string CreatingUserId { get; set; }
        [MaxLength(20)]
        public string AffectedUserId { get; set; }
        public int PointDelta { get; set; }
        
        public string Message { get; set; }
        
        public string ChannelId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }
    }
}