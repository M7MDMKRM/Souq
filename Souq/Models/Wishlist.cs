using System;
using System.ComponentModel.DataAnnotations;

namespace Souq.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        
        public DateTime AddedDate { get; set; } = DateTime.Now;

        public virtual Product? Product { get; set; }
    }
}
