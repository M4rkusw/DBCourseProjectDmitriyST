using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy.Repositories
{
	/// <summary>
	/// Репозиторий для клиентов
	/// </summary>
	public class ClientRepository : Repository<Client>
	{
		public ClientRepository(WebApplication app, string entities) : base(app, entities) { }

		protected override DbSet<Client> DbSet => _db.Clients;
		protected override void Change()
		{
			app.MapPut($"/api/{entities}", async (Client client) =>
			{
				// получаем пользователя по id
				var oldClient = await DbSet.FirstOrDefaultAsync(u => u.Id == client.Id);

				// если не найден, отправляем статусный код и сообщение об ошибке
				if (oldClient == null) return Results.NotFound(new { message = "Пользователь не найден" });

				// если пользователь найден, изменяем его данные и отправляем обратно клиенту
				oldClient.FullName = client.FullName;
				oldClient.Telephone = client.Telephone;
				oldClient.BankAccount = client.BankAccount;
				await _db.SaveChangesAsync();
				return Results.Json(oldClient);
			});
		}

		protected override void Unload() { }
	}
}
