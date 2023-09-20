using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy.Repositories
{
	public class ElectronicsTypeRepository : Repository<ElectronicsType>
	{
		public ElectronicsTypeRepository(WebApplication app, string entities) : base(app, entities) { }

		protected override DbSet<ElectronicsType> DbSet => _db.ElectronicsTypes;

		protected override void Change()
		{

		}

		protected override void Unload() { }
	}
}
