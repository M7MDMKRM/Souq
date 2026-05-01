using System;
using System.Collections.Generic;

namespace Souq.Models;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? Catid { get; set; }

    public string? Photo { get; set; }
    public bool IsFeatured { get; set; } = false;
}


public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Photo { get; set; }

    public string? Description { get; set; }
}

