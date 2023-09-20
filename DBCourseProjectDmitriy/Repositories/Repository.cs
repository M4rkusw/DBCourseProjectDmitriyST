using ClosedXML.Excel;
using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy.Repositories
{
    /// <summary>
    /// Обобщенный репозиторий для работы с сущностями IEntity
    /// </summary>
    /// <typeparam name="TEntity">Сущность, реализующая IEntity</typeparam>
    public abstract class Repository<TEntity> where TEntity : class, IEntity
    {
        protected readonly string entities;
        protected readonly Context _db;
        protected abstract DbSet<TEntity> DbSet { get; }
        protected readonly WebApplication app;

        protected Repository(WebApplication app, string entities)
        {
            this.entities = entities;
            _db = app.Services.CreateScope().ServiceProvider.GetServices<Context>().First();
            this.app = app;

            GetAll();
            GetById();
            Add();
            Delete();
            Change();
            Unload();
        }

        /// <summary>
        /// Запрос на получение всех сущностей из БД
        /// </summary>
        protected virtual void GetAll()
        {
            app.MapGet($"/api/{entities}", async () => await DbSet.ToListAsync());
        }

        /// <summary>
        /// Запрос на получение сущности по id из БД
        /// </summary>
        protected virtual void GetById()
        {
            app.MapGet($"/api/{entities}/{{id:int}}", async (int id) =>
            {
                //Получаем сущность по id
                TEntity? entity = await DbSet.FirstOrDefaultAsync(u => u.Id == id);

                //Если не найдена, отправляем статусный код и сообщение об ошибке
                if (entity == null) return Results.NotFound(new { message = $"Сущность {entity} не найдена" });

                //Если сущность найдена, отправляем её
                return Results.Json(entity);
            });
        }

        /// <summary>
        /// Запрос на добавлении сущности в БД
        /// </summary>
        protected virtual void Add()
        {
            app.MapPost($"/api/{entities}", async (TEntity entity) =>
            {
                // добавляем сущность в массив
                await DbSet.AddAsync(entity);
                await _db.SaveChangesAsync();
                return Results.Json(entity);
            });
        }

		/// <summary>
		/// Запрос на удаление сущности из БД
		/// </summary>
		protected virtual void Delete()
        {
            app.MapDelete($"/api/{entities}/{{id:int}}", async (int id) =>
            {
                // получаем сущность по id
                TEntity? entity = await DbSet.FirstOrDefaultAsync(u => u.Id == id);

                // если не найдена, отправляем статусный код и сообщение об ошибке
                if (entity == null) return Results.NotFound(new { message = $"Сущность {entity} не найдена" });

                // если сущность найдена, удаляем её
                DbSet.Remove(entity);
                await _db.SaveChangesAsync();
                return Results.Json(entity);
            });
        }

        /// <summary>
        /// Запрос на выгрузку данных в excel таблицу 
        /// </summary>
        protected abstract void Unload();

        /// <summary>
        /// Запрос на изменение сущности в БД
        /// </summary>
        protected abstract void Change();
    }
}
