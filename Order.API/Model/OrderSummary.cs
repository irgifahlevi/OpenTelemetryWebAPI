namespace Order.API.Model
{
    public class OrderSummary
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int OrderedQuantity { get; set; }
        public decimal TotalAmount => ProductPrice * OrderedQuantity;
    }
}
