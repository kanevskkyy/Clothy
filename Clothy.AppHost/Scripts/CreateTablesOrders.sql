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

CREATE TABLE regions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    cityid UUID NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    FOREIGN KEY (cityid) REFERENCES city(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    UNIQUE (name, cityid)
);

CREATE TABLE settlements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    regionid UUID NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    FOREIGN KEY (regionid) REFERENCES regions(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    UNIQUE (name, regionid)
);

CREATE TABLE pickup_points (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    address VARCHAR(100) NOT NULL,
    deliveryproviderid UUID NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    FOREIGN KEY (deliveryproviderid) REFERENCES delivery_provider(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    UNIQUE (address, deliveryproviderid)
);

CREATE TABLE delivery_detail (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    orderid UUID NOT NULL,
    pickuppointid UUID NOT NULL,
    phonenumber VARCHAR(20) NOT NULL,
    firstname VARCHAR(100) NOT NULL,
    lastname VARCHAR(100) NOT NULL,
    middlename VARCHAR(100) NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    CONSTRAINT fk_deliverydetail_order FOREIGN KEY (orderid)
        REFERENCES orders(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT fk_pickuppoints_id FOREIGN KEY (pickuppointid)
        REFERENCES pickup_points(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE INDEX idx_order_statusid ON orders(statusid);
CREATE INDEX idx_order_userid ON orders(userid);
CREATE INDEX idx_orderitem_orderid ON order_item(orderid);
CREATE INDEX idx_regions_cityid ON regions(cityid);
CREATE INDEX idx_settlements_regionid ON settlements(regionid);
CREATE INDEX idx_pickup_points_providerid ON pickup_points(deliveryproviderid);
CREATE INDEX idx_deliverydetail_orderid ON delivery_detail(orderid);
CREATE INDEX idx_deliverydetail_pickuppointsid ON delivery_detail(pickuppointid);