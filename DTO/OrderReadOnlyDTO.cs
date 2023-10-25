namespace WebApiApp.DTO
{
    public class OrderReadOnlyDTO
    {
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }
        public DateTime? Date { get; set; }

        public string? CustomerName { get; set; }

    }
}
