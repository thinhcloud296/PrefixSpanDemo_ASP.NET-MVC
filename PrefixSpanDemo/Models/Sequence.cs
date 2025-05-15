using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PrefixSpanDemo.Models
{
    public class Sequence
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string SessionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime ClickTime { get; set; } = DateTime.Now;

        public int SequenceOrder { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }    
    }
}