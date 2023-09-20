using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System.Text.Json;

namespace DBCourseProjectDmitriy.Repositories
{
	public class ContractRepository : Repository<Contract>
	{
		public ContractRepository(WebApplication app, string entities) : base(app, entities)
		{
		}

		protected override DbSet<Contract> DbSet => _db.Contracts;

		protected override void GetAll()
		{
			app.MapGet($"/api/{entities}", async () =>
			{
				List<Contract> entities = await DbSet.Include(p => p.Manufacturer).Include(p => p.Client).ToListAsync();
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
				Contract? entity = await DbSet.FirstOrDefaultAsync(u => u.Id == id);

				if (entity == null) return Results.NotFound(new { message = $"Договор {entity} не найдена" });

				return Results.Json(entity,
					new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					}.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
			});
		}

		protected override void Add()
		{
			app.MapPost($"/api/{entities}", async (Contract entity) =>
			{
				DateTime date = DateTime.Parse(entity.RealDate);
				entity.Date = Instant.FromUtc(date.Year, date.Month, date.Day, date.Hour, date.Minute);
				entity.Client = entity.Client == null ? null : _db.Clients.Where(p => p.Id == entity.Client.Id).First();
				entity.Manufacturer = entity.Manufacturer == null ? null : _db.Manufacturers.Where(p => p.Id == entity.Manufacturer.Id).First();
				// добавляем сущность в массив
				await DbSet.AddAsync(entity);
				await _db.SaveChangesAsync();
				return Results.Json(entity);
			});
		}

		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (Contract entity) =>
			{
				var oldContracts = await DbSet.FirstOrDefaultAsync(u => u.Id == entity.Id);

				if (oldContracts == null) return Results.NotFound(new { message = "Договор не найден" });

				DateTime date = DateTime.Parse(entity.RealDate);
				oldContracts.Date = Instant.FromUtc(date.Year, date.Month, date.Day, date.Hour, date.Minute);
				oldContracts.IsSupply = entity.IsSupply;
				oldContracts.Client = !entity.IsSupply ? _db.Clients.Where(p => p.Id == entity.Client.Id).First() : null;
				oldContracts.Manufacturer = entity.IsSupply ? _db.Manufacturers.Where(p => p.Id == entity.Manufacturer.Id).First() : null;
				await _db.SaveChangesAsync();
				return Results.Json(oldContracts);
			});
		}

		protected override void Unload()
		{
			app.MapPost($"/api/{entities}/unload", async () =>
			{
				using var workBook = File.Exists("Report.xlsx") ? new XLWorkbook("Report.xlsx") : new XLWorkbook();

				string worksheetName;
				worksheetName = "Договоры на поставку";

				List<Contract> contracts = await DbSet.Include(p => p.Client).Include(p => p.Manufacturer).ToListAsync();

				for (int j = 0; j < 2; j++)
				{
					if (workBook.Worksheets.Contains(worksheetName))
					{
						workBook.Worksheet(worksheetName).Delete();
					}

					var workSheet = workBook.Worksheets.Add(worksheetName);

					workSheet.Cell("A" + 1).Value = "Id";
					workSheet.Cell("B" + 1).Value = "Дата заключения";
					workSheet.Cell("C" + 1).Value = "Заключен с";
					int indent = 2;

					for (int i = 0; i < contracts.Count; i++)
					{
						if (j == 0)
						{
							if (!contracts[i].IsSupply)
							{
								continue;
							}
						}
						else
						{
							if (contracts[i].IsSupply)
							{
								continue;
							}
						}

						workSheet.Cell("A" + indent).Value = contracts[i].Id;
						workSheet.Cell("B" + indent).Value = contracts[i].Date.ToDateTimeUtc().Date;

						if (j == 0)
						{
							workSheet.Cell("C" + indent).Value = contracts[i].Manufacturer.Name;
						}
						else
						{
							workSheet.Cell("C" + indent).Value = contracts[i].Client.FullName;
						}

						indent++;
					}

					workSheet.Columns().AdjustToContents();

					worksheetName = "Договоры на продажу";
				}

				workBook.SaveAs("Report.xlsx");
			});
		}
	}
}
