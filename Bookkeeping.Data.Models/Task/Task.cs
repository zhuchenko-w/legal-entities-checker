using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookkeeping.Data.Models
{
    [Table("Tasks", Schema = "public")]
    public class Task
    {
        public int Id { get; set; }
        public string Inn { get; set; }
        public string Name { get; set; }
        public string PurposeOfPayment { get; set; }
        public DateTime Date { get; set; }
        public int TotalCount { get; set; }
        public int? ClientId { get; set; }
        public bool IsArchive { get; set; }
    }
}