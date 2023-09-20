using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;

namespace DBCourseProjectDmitriy.Repositories
{
	public class OrderedGoodsRepository : Repository<OrderedGoods>
	{
		public OrderedGoodsRepository(WebApplication app, string entities) : base(app, entities)
		{
			GetOrderedGoodsFromPurchaseOrder();
		}

		protected override DbSet<OrderedGoods> DbSet => _db.OrderedGoods;
		protected override void Add()
		{
			app.MapPost($"/api/{entities}", async (OrderedGoods entity) =>
			{
				entity.Electronics = _db.Electronics.Where(p => p.Id == entity.Electronics.Id).First();
				// добавляем сущность в массив
				await DbSet.AddAsync(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected void GetOrderedGoodsFromPurchaseOrder()
		{
			app.MapGet($"/api/{entities}/fromPurchaseOrder/{{purchaseOrderId:int}}",
				async (int purchaseOrderId) => await DbSet
					.Where(p => p.PurchaseOrder.Id == purchaseOrderId)
					.Include(p => p.PurchaseOrder)
					.Include(p => p.Electronics)
					.Include(p => p.Electronics.ElectronicsType)
					.ToListAsync());
		}

		protected override void GetAll()
		{
			app.MapGet($"/api/{entities}", async () =>
			{
				List<OrderedGoods> entities = await DbSet
					.Include(p => p.Electronics)
					.Include(p => p.PurchaseOrder)
					.Include(p => p.PurchaseOrder.Contract)
					.Include(p => p.Electronics.ElectronicsType)
					.Include(p => p.Electronics.Manufacturer)
					.ToListAsync();

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
				OrderedGoods? entity = await DbSet.FindAsync(id);
				//.Include(p => p.PurchaseOrder)
				//.Include(p => p.PurchaseOrder.Contract)
				//.Include(p => p.PurchaseOrder.Contract.Manufacturer)
				//.Include(p => p.PurchaseOrder.Contract.Client)
				//.FirstOrDefaultAsync(u => u.Id == id);

				if (entity == null) return Results.NotFound(new { message = $"Сущность {entity} не найдена" });

				return Results.Json(entity,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (OrderedGoods entity) =>
			{
				var oldOrderedGood = await DbSet.FindAsync(entity.Id);

				if (oldOrderedGood == null) return Results.NotFound(new { message = "Заказ не найден" });

				var electronics = await _db.Electronics.FindAsync(entity.Electronics.Id);

				if (electronics == null) return Results.NotFound(new { message = "Товар не найден" });

				oldOrderedGood.Quantity = entity.Quantity;
				oldOrderedGood.Electronics = electronics;
				oldOrderedGood.Sum = entity.Quantity * electronics.Price;
				oldOrderedGood.PurchaseOrder = (entity.PurchaseOrder == null) ? null : await _db.PurchaseOrders.FindAsync(entity.PurchaseOrder.Id);
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

				string worksheetName = "Заказываемые товары";

				if (workBook.Worksheets.Contains(worksheetName))
				{
					workBook.Worksheet(worksheetName).Delete();
				}

				var workSheet = workBook.Worksheets.Add(worksheetName);

				workSheet.Cell("A" + 1).Value = "Id";
				workSheet.Cell("B" + 1).Value = "Название телевизора";
				workSheet.Cell("C" + 1).Value = "Количество";
				workSheet.Cell("D" + 1).Value = "Сумма";
				int indent = 2;

				List<OrderedGoods> goods = await DbSet.Where(p => p.PurchaseOrder == null).Include(p => p.Electronics).ToListAsync();

				for (int i = 0; i < goods.Count; i++)
				{
					workSheet.Cell("A" + indent).Value = goods[i].Id;
					workSheet.Cell("B" + indent).Value = goods[i].Electronics.Name;
					workSheet.Cell("C" + indent).Value = goods[i].Quantity;
					workSheet.Cell("D" + indent).Value = goods[i].Sum;

					indent++;
				}

				workSheet.Columns().AdjustToContents();

				workBook.SaveAs("Report.xlsx");
			});
		}
	}
}
