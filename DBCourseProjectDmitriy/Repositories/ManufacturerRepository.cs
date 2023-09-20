using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy.Repositories
{
	/// <summary>
	/// Репозиторий для производителей
	/// </summary>
	public class ManufacturerRepository : Repository<Manufacturer>
	{
		public ManufacturerRepository(WebApplication app, string entities) : base(app, entities) { }
		protected override DbSet<Manufacturer> DbSet => _db.Manufacturers;
		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (Manufacturer manufacturer) =>
			{
				var oldManufacturer = await DbSet.FirstOrDefaultAsync(u => u.Id == manufacturer.Id);

				if (oldManufacturer == null) return Results.NotFound(new { message = "Производитель не найден" });

				oldManufacturer.Name = manufacturer.Name;
				oldManufacturer.Director = manufacturer.Director;
				oldManufacturer.BankDetails = manufacturer.BankDetails;
				await _db.SaveChangesAsync();
				return Results.Json(oldManufacturer);
			});
		}

		protected override void Unload() { }
	}
}
