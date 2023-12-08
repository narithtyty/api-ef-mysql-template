using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.MySQL.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }
}
