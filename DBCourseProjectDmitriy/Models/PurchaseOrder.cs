namespace DBCourseProjectDmitriy.Models
{
    public class PurchaseOrder : IEntity
    {
        public int Id { get; set; }
        public bool PaymentStat { get; set; } = false;
        public uint Sum { get; set; }
        public Contract Contract { get; set; }
    }
}
