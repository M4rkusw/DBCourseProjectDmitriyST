using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy.Repositories
{
	public class ElectronicsRepository : Repository<Electronics>
	{
		public ElectronicsRepository(WebApplication app, string entities) : base(app, entities) { }

		protected override DbSet<Electronics> DbSet => _db.Electronics;

		protected override void GetAll()
		{
			app.MapGet($"/api/{entities}", async () => await DbSet.Include(p => p.ElectronicsType).Include(p => p.Manufacturer).ToListAsync());
		}

		protected override void Add()
		{
			app.MapPost($"/api/{entities}", async (Electronics entity) =>
			{
				entity.ElectronicsType = _db.ElectronicsTypes.Where(p => p.Id == entity.ElectronicsType.Id).First();
				entity.Manufacturer = _db.Manufacturers.Where(p => p.Id == entity.Manufacturer.Id).First();
				// добавляем сущность в массив
				await DbSet.AddAsync(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (Electronics entity) =>
			{
				var oldElectronics = await DbSet.FindAsync(entity.Id);

				if (oldElectronics == null) return Results.NotFound(new { message = "Электроника не найден" });

				oldElectronics.Name = entity.Name;
				oldElectronics.Price = entity.Price;
				oldElectronics.ElectronicsInStock = entity.ElectronicsInStock;
				oldElectronics.SoldNumber = entity.SoldNumber;
				oldElectronics.DeliveredNumber = entity.DeliveredNumber;
				oldElectronics.ElectronicsType = _db.ElectronicsTypes.Where(p => p.Id == entity.ElectronicsType.Id).First();
				oldElectronics.Manufacturer = _db.Manufacturers.Where(p => p.Id == entity.Manufacturer.Id).First();

				DbSet.Update(oldElectronics);

				//_db.Entry(oldElectronics).State = EntityState.Modified;
				//DbSet.Attach(oldElectronics);
				//_db.ChangeTracker.DetectChanges();
				await _db.SaveChangesAsync();
				return Results.Json(oldElectronics);
			});
		}

		protected override void Unload()
		{
			app.MapPost($"/api/{entities}/unload", async () =>
			{
				using var workBook = File.Exists("Report.xlsx") ? new XLWorkbook("Report.xlsx") : new XLWorkbook();

				List<Electronics> electronics = await DbSet.Include(p => p.ElectronicsType).Include(p => p.Manufacturer).ToListAsync();

				BookEntry("Товары", workBook, electronics);
				BookEntry("Товары на складе", workBook, electronics, 1);
				BookEntry("Проданные товары", workBook, electronics, 2);
				BookEntry("Поставленные товары", workBook, electronics, 3);

				workBook.SaveAs("Report.xlsx");
			});
		}

		private void BookEntry(string workSheetName, XLWorkbook workBook, List<Electronics> electronics, int flag = 0)
		{
			int indent = 2;

			if (workBook.Worksheets.Contains(workSheetName))
			{
				workBook.Worksheet(workSheetName).Delete();
			}

			var workSheet = workBook.Worksheets.Add(workSheetName);

			workSheet.Cell("A" + 1).Value = "Id";
			workSheet.Cell("B" + 1).Value = "Название";
			workSheet.Cell("C" + 1).Value = "Цена";
			workSheet.Cell("D" + 1).Value = "Тип";
			workSheet.Cell("E" + 1).Value = "Производитель";
			switch (flag)
			{
				case 0:
					break;
				case 1:
					workSheet.Cell("F" + 1).Value = "Количество на складе";
					break;
				case 2:
					workSheet.Cell("F" + 1).Value = "Продано";
					break;
				case 3:
					workSheet.Cell("F" + 1).Value = "Поставленно";
					break;

			}

			for (int i = 0; i < electronics.Count; i++)
			{
				switch (flag)
				{
					case 0:
						break;
					case 1:
						if (electronics[i].ElectronicsInStock <= 0) continue;
						workSheet.Cell("F" + indent).Value = electronics[i].ElectronicsInStock;
						break;
					case 2:
						if (electronics[i].SoldNumber <= 0) continue;
						workSheet.Cell("F" + indent).Value = electronics[i].SoldNumber;
						break;
					case 3:
						if (electronics[i].DeliveredNumber <= 0) continue;
						workSheet.Cell("F" + indent).Value = electronics[i].DeliveredNumber;
						break;
				}

				workSheet.Cell("A" + indent).Value = electronics[i].Id;
				workSheet.Cell("B" + indent).Value = electronics[i].Name;
				workSheet.Cell("C" + indent).Value = electronics[i].Price;
				workSheet.Cell("D" + indent).Value = electronics[i].ElectronicsType.Type;
				workSheet.Cell("E" + indent).Value = electronics[i].Manufacturer.Name;

				indent++;
			}

			workSheet.Columns().AdjustToContents();
		}
	}
}
