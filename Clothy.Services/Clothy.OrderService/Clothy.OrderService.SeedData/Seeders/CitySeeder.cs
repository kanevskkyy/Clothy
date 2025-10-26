using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.SeedData.Seeders
{
    public class CitySeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<City> existingCities = await uow.Cities.GetAllAsync();
            if (existingCities.Any()) return; 

            List<string> cities = new List<string>
            {
                "Kyiv",
                "Lviv",
                "Odesa",
                "Kharkiv",
                "Dnipro",
                "Chernivtsi",
                "Ivano-Frankivsk",
                "Vinnytsia",
                "Uzhhorod",
                "Zaporizhzhia"
            };

            Faker faker = new Faker();

            foreach (string cityName in cities)
            {
                City city = new City
                {
                    Name = cityName,
                    CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.Cities.AddWithoutReturningAsync(city);
            }
            await uow.CommitAsync();
        }
    }
}
