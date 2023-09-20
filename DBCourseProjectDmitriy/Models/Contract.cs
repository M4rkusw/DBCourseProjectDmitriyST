using NodaTime;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBCourseProjectDmitriy.Models
{
    public class Contract : IEntity
    {
        public int Id { get; set; }
        public Instant Date { get; set; }
        [NotMapped]
        public string RealDate { get; set; }
        public bool IsSupply { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public Client? Client { get; set; }
    }
}
