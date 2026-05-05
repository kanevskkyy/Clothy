using Clothy.OrderService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Clothy.OrderService.ContractTests.Infrastructure;

public static class OrderContractSeedHelper
{
    public static async Task<SeedData> SeedOrderAsync(string connectionString)
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        Guid colorId = Guid.NewGuid();
        Guid sizeId = Guid.NewGuid();

        await using NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        await using NpgsqlCommand cmd = new(@"
            INSERT INTO orders (id, status, userid, userfirstname, userlastname, useremail, createdat)
            VALUES (@id, 0, @userId, 'John', 'Doe', 'john@test.com', NOW());

            INSERT INTO order_item (id, orderid, clotheid, clothename, price, mainphoto,
                colorid, hexcode, sizeid, sizename, quantity, createdat)
            VALUES (@itemId, @id, @clotheId, 'Contract Clothe', 100.00,
                'https://test.com/photo.jpg', @colorId, '#000000', @sizeId, 'M', 1, NOW());
        ", conn);

        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@itemId", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@clotheId", clotheId);
        cmd.Parameters.AddWithValue("@colorId", colorId);
        cmd.Parameters.AddWithValue("@sizeId", sizeId);

        await cmd.ExecuteNonQueryAsync();

        return new SeedData(orderId, userId, clotheId);
    }

    public static async Task CleanAsync(string connectionString)
    {
        await using NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        await using NpgsqlCommand cmd = new(
            "DELETE FROM order_item; DELETE FROM orders;", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public record SeedData(Guid OrderId, Guid UserId, Guid ClotheId);
}