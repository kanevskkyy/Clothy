SELECT 'CREATE DATABASE "ClothyOrder"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ClothyOrder')\gexec

\c ClothyOrder


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE order_status (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    iconurl TEXT NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE
);

CREATE TABLE delivery_provider (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    iconurl TEXT NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE
);

CREATE TABLE city (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE
);

CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    statusid UUID NOT NULL,
    userid UUID NOT NULL,
    userfirstname VARCHAR(100) NOT NULL,
    userlastname VARCHAR(100) NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    CONSTRAINT fk_order_status FOREIGN KEY (statusid)
        REFERENCES order_status(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE order_item (
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
    CONSTRAINT fk_orderitem_order FOREIGN KEY (orderid)
        REFERENCES orders(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE delivery_detail (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    orderid UUID NOT NULL,
    providerid UUID NOT NULL,
    cityid UUID NOT NULL,
    postalindex VARCHAR(20) NOT NULL,
    phonenumber VARCHAR(20) NOT NULL,
    firstname VARCHAR(100) NOT NULL,
    lastname VARCHAR(100) NOT NULL,
    middlename VARCHAR(100) NOT NULL,
    detailsdescription TEXT NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    CONSTRAINT fk_deliverydetail_order FOREIGN KEY (orderid)
        REFERENCES orders(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_deliverydetail_provider FOREIGN KEY (providerid)
        REFERENCES delivery_provider(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_deliverydetail_city FOREIGN KEY (cityid)
        REFERENCES city(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE INDEX idx_order_statusid ON orders(statusid);
CREATE INDEX idx_order_userid ON orders(userid);
CREATE INDEX idx_orderitem_orderid ON order_item(orderid);
CREATE INDEX idx_deliverydetail_orderid ON delivery_detail(orderid);
CREATE INDEX idx_deliverydetail_providerid ON delivery_detail(providerid);
CREATE INDEX idx_deliverydetail_cityid ON delivery_detail(cityid);
