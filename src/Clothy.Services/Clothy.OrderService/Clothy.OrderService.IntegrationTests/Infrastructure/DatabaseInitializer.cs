using Npgsql;

namespace Clothy.OrderService.IntegrationTests.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand command = new NpgsqlCommand(@"
            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";

            CREATE TABLE IF NOT EXISTS delivery_provider (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                name VARCHAR(100) NOT NULL UNIQUE,
                iconurl TEXT NOT NULL,
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE
            );

            CREATE TABLE IF NOT EXISTS orders (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                status SMALLINT NOT NULL DEFAULT 0,
                userid UUID NOT NULL,
                userfirstname VARCHAR(100) NOT NULL,
                userlastname VARCHAR(100) NOT NULL,
                comment VARCHAR(80),
                useremail VARCHAR(100) NOT NULL CHECK (useremail LIKE '%@%'),
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE
            );

            CREATE TABLE IF NOT EXISTS order_item (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                orderid UUID NOT NULL,
                clotheid UUID NOT NULL,
                clothename VARCHAR(200) NOT NULL,
                price NUMERIC(10,2) NOT NULL CHECK (price >= 0),
                mainphoto TEXT NOT NULL,
                colorid UUID NOT NULL,
                hexcode VARCHAR(7) NOT NULL CHECK (hexcode ~ '^#[0-9A-Fa-f]{6}$'),
                sizeid UUID NOT NULL,
                sizename VARCHAR(50) NOT NULL,
                quantity INT NOT NULL CHECK (quantity > 0),
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE,
                isclothedeleted BOOLEAN NOT NULL DEFAULT false,
                isclotheupdated BOOLEAN NOT NULL DEFAULT false,
                CONSTRAINT fk_orderitem_order FOREIGN KEY (orderid)
                    REFERENCES orders(id) ON UPDATE CASCADE ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS regions (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                name VARCHAR(100) NOT NULL,
                ref VARCHAR(100) NOT NULL,
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE,
                UNIQUE (name)
            );

            CREATE TABLE IF NOT EXISTS settlements (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                name VARCHAR(100) NOT NULL,
                type SMALLINT NOT NULL,
                regionid UUID NOT NULL,
                ref VARCHAR(100) NOT NULL,
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE,
                FOREIGN KEY (regionid) REFERENCES regions(id) ON UPDATE CASCADE ON DELETE CASCADE,
                UNIQUE (name, regionid)
            );

            CREATE TABLE IF NOT EXISTS pickup_points (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                address VARCHAR(100) NOT NULL,
                ref VARCHAR(100) NOT NULL,
                deliveryproviderid UUID NOT NULL,
                settlementid UUID NOT NULL,
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE,
                isactive BOOLEAN DEFAULT TRUE,
                FOREIGN KEY (deliveryproviderid) REFERENCES delivery_provider(id) ON UPDATE CASCADE ON DELETE CASCADE,
                FOREIGN KEY (settlementid) REFERENCES settlements(id) ON UPDATE CASCADE ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS processed_events (
                eventid UUID PRIMARY KEY,
                processedat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc')
            );

            CREATE TABLE IF NOT EXISTS delivery_detail (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                orderid UUID NOT NULL,
                pickuppointid UUID NOT NULL,
                phonenumber VARCHAR(20) NOT NULL,
                firstname VARCHAR(100) NOT NULL,
                lastname VARCHAR(100) NOT NULL,
                email VARCHAR(100) NOT NULL CHECK (email LIKE '%@%'),
                createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
                updatedat TIMESTAMP WITHOUT TIME ZONE,
                CONSTRAINT fk_deliverydetail_order FOREIGN KEY (orderid)
                    REFERENCES orders(id) ON UPDATE CASCADE ON DELETE CASCADE,
                CONSTRAINT fk_pickuppoints_id FOREIGN KEY (pickuppointid)
                    REFERENCES pickup_points(id) ON UPDATE CASCADE ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS orders_reservations (
                id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                orderid UUID NOT NULL,
                clotheid UUID NOT NULL,
                colorid UUID NOT NULL,
                sizeid UUID NOT NULL,
                quantity INTEGER NOT NULL,
                reservedat TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
                expiresat TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                isactive BOOLEAN DEFAULT true,
                CONSTRAINT fk_orderid_order FOREIGN KEY (orderid)
                    REFERENCES orders(id) ON UPDATE CASCADE ON DELETE CASCADE
            );
        ", connection);

        await command.ExecuteNonQueryAsync();
    }
}