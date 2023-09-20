using NodaTime;

namespace DBCourseProjectDmitriy.Models
{
    public class Bill : IEntity
    {
        public int Id { get; set; }
        public Instant Date { get; set; } = Instant.FromUtc(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
        public uint Sum { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
    }
}
