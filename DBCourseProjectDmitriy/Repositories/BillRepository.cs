using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;

namespace DBCourseProjectDmitriy.Repositories
{
	public class BillRepository : Repository<Bill>
	{
		public BillRepository(WebApplication app, string entities) : base(app, entities)
		{
		}

		protected override void GetAll()
		{
			app.MapGet($"/api/{entities}", async () =>
			{
				List<Bill> entities = await DbSet
					.Include(p => p.PurchaseOrder.Contract)
					.Include(p => p.PurchaseOrder.Contract.Manufacturer)
					.Include(p => p.PurchaseOrder.Contract.Client)
					.ToListAsync();
				return Results.Json(entities,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void Add()
		{
			app.MapPost($"/api/{entities}", async (Bill entity) =>
			{
				entity.Date = Instant.FromUtc(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
				entity.Sum = entity.Sum;
				entity.PurchaseOrder = _db.PurchaseOrders.Where(p => p.Id == entity.PurchaseOrder.Id).First();
				// добавляем сущность в массив
				await DbSet.AddAsync(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected override DbSet<Bill> DbSet => _db.Bills;

		protected override void Change()
		{

		}

		protected override void Unload()
		{
			app.MapPost($"/api/{entities}/unload", async () =>
			{
				using var workBook = File.Exists("Report.xlsx") ? new XLWorkbook("Report.xlsx") : new XLWorkbook();

				string worksheetName = "Чеки";

				if (workBook.Worksheets.Contains(worksheetName))
				{
					workBook.Worksheet(worksheetName).Delete();
				}

				var workSheet = workBook.Worksheets.Add(worksheetName);

				workSheet.Cell("A" + 1).Value = "Id";
				workSheet.Cell("B" + 1).Value = "Время оплаты";
				workSheet.Cell("C" + 1).Value = "Сумма оплаты";
				workSheet.Cell("D" + 1).Value = "Номер заказа";

				List<Bill> bills = await DbSet.Include(p => p.PurchaseOrder).ToListAsync();

				for (int i = 0; i < bills.Count; i++)
				{
					workSheet.Cell("A" + (i + 2)).Value = bills[i].Id;
					workSheet.Cell("B" + (i + 2)).Value = bills[i].Date.ToDateTimeUtc().Date;
					workSheet.Cell("C" + (i + 2)).Value = bills[i].Sum;
					workSheet.Cell("D" + (i + 2)).Value = bills[i].PurchaseOrder.Id;
				}

				workSheet.Columns().AdjustToContents();

				workBook.SaveAs("Report.xlsx");
			});
		}
	}
}
