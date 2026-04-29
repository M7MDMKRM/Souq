using System;
using System.Collections.Generic;
namespace Souq.Models;

public partial class Cart
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; } = 1;
    public string? UserId { get; set; }

    public virtual Product? Product { get; set; }
}