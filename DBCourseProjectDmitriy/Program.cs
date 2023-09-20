using DBCourseProjectDmitriy.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace DBCourseProjectDmitriy
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllersWithViews().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
			});

			string connection = builder.Configuration.GetConnectionString("DefaultConnection");
			builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connection, o => o.UseNodaTime()));

			var app = builder.Build();

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseDefaultFiles();
			app.UseStaticFiles();

			ClientRepository clientRepository = new(app, "clients");
			ManufacturerRepository manufacturerRepository = new(app, "manufacturers");
			ElectronicsRepository electronicsRepository = new(app, "electronics");
			ElectronicsTypeRepository electronicsTypeRepository = new(app, "electronicsType");
			ContractRepository contractRepository = new(app, "contracts");
			OrderedGoodsRepository orderedGoodsRepository = new(app, "orderedGoods");
			PurchaseOrderRepository purchaseOrderRepository = new(app, "purchaseOrder");
			BillRepository billRepository = new(app, "bill");

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Manufacturer}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Electronics}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Contract}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Cart}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Orders}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Bills}/{id?}");

			app.Run();
		}
	}
}