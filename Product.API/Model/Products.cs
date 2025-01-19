﻿namespace Product.API.Model
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }

    }
}
