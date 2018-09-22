using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [Table("Agents", Schema = "public")]
    public class Agent
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool? IsLocked { get; set; }
    }
}