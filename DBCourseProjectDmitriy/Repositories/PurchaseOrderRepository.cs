using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;

namespace DBCourseProjectDmitriy.Repositories
{
	public class PurchaseOrderRepository : Repository<PurchaseOrder>
	{
		public PurchaseOrderRepository(WebApplication app, string entities) : base(app, entities)
		{
		}

		protected override DbSet<PurchaseOrder> DbSet => _db.PurchaseOrders;

		protected override void GetAll()
		{
			app.MapGet($"/api/{entities}", async () =>
			{
				List<PurchaseOrder> entities = await DbSet.Include(p => p.Contract).Include(p => p.Contract.Manufacturer).Include(p => p.Contract.Client).ToListAsync();
				return Results.Json(entities,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void GetById()
		{
			app.MapGet($"/api/{entities}/{{id:int}}", async (int id) =>
			{
				PurchaseOrder? entity = await DbSet.FindAsync(id);

				if (entity == null) return Results.NotFound(new { message = $"Сущность {entity} не найдена" });

				return Results.Json(entity,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void Add()
		{
			app.MapPost($"/api/{entities}", async (PurchaseOrder entity) =>
			{
				Console.WriteLine(entity.Contract.Id);
				entity.Contract = _db.Contracts.Where(p => p.Id == entity.Contract.Id).First();

				// добавляем сущность в массив
				await DbSet.AddAsync(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected override void Delete()
		{
			app.MapDelete($"/api/{entities}/{{id:int}}", async (int id) =>
			{
				PurchaseOrder? entity = await DbSet.FirstOrDefaultAsync(u => u.Id == id);

				if (entity == null) return Results.NotFound(new { message = $"Сущность {entity} не найдена" });

				List<OrderedGoods> goods = await _db.OrderedGoods.Where(p => p.PurchaseOrder.Id == entity.Id).ToListAsync();

				foreach (OrderedGoods good in goods)
				{
					_db.OrderedGoods.Remove(good);
				}

				await _db.SaveChangesAsync();

				DbSet.Remove(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (PurchaseOrder entity) =>
			{
				var oldPurchaseOrder = await DbSet.FindAsync(entity.Id);

				if (oldPurchaseOrder == null) return Results.NotFound(new { message = "Заказ не найден" });

				var contract = await _db.Contracts.FindAsync(entity.Contract.Id);

				if (contract == null) return Results.NotFound(new { message = "Договор не найден" });

				oldPurchaseOrder.PaymentStat = entity.PaymentStat;
				oldPurchaseOrder.Sum = entity.Sum;
				oldPurchaseOrder.Contract = contract;
				await _db.SaveChangesAsync();
				return Results.Json(entity,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void Unload()
		{
			app.MapPost($"/api/{entities}/unload", async () =>
			{
				using var workBook = File.Exists("Report.xlsx") ? new XLWorkbook("Report.xlsx") : new XLWorkbook();

				string worksheetName = "Неоплаченные заказы";

				if (workBook.Worksheets.Contains(worksheetName))
				{
					workBook.Worksheet(worksheetName).Delete();
				}

				var workSheet = workBook.Worksheets.Add(worksheetName);

				workSheet.Cell("A" + 1).Value = "Id";
				workSheet.Cell("B" + 1).Value = "Сумма заказа";
				workSheet.Cell("C" + 1).Value = "Номер договора";
				int indent = 2;

				List<PurchaseOrder> purchaseOrders = await DbSet.Where(p => !p.PaymentStat).Include(p => p.Contract).ToListAsync();

				for (int i = 0; i < purchaseOrders.Count; i++)
				{
					workSheet.Cell("A" + indent).Value = purchaseOrders[i].Id;
					workSheet.Cell("B" + indent).Value = purchaseOrders[i].Sum;
					workSheet.Cell("C" + indent).Value = purchaseOrders[i].Contract.Id;

					indent++;
				}

				workSheet.Columns().AdjustToContents();

				workBook.SaveAs("Report.xlsx");
			});
		}
	}
}
