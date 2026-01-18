SELECT 'CREATE DATABASE "ClothyOrder"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ClothyOrder')\gexec

\c ClothyOrder


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE order_status (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
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

CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    statusid UUID NOT NULL,
    userid UUID NOT NULL,
    userfirstname VARCHAR(100) NOT NULL,
    userlastname VARCHAR(100) NOT NULL,
    comment VARCHAR(80),
    useremail VARCHAR(100) NOT NULL CHECK (useremail LIKE '%@%'),
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
    isclothedeleted BOOLEAN NOT NULL DEFAULT false,
    isclotheupdated BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_orderitem_order FOREIGN KEY (orderid)
        REFERENCES orders(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE regions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    ref VARCHAR(100) NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    UNIQUE (name)
);

CREATE TABLE settlements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    type SMALLINT NOT NULL,
    regionid UUID NOT NULL,
    ref VARCHAR(100) NOT NULL,
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
    ref VARCHAR(100) NOT NULL,
    deliveryproviderid UUID NOT NULL,
    settlementid UUID NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc'),
    updatedat TIMESTAMP WITHOUT TIME ZONE,
    isactive bool DEFAULT TRUE,
    FOREIGN KEY (deliveryproviderid) REFERENCES delivery_provider(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    FOREIGN KEY (settlementid) REFERENCES settlements(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE processed_events(
    eventid UUID PRIMARY KEY,
    processedat TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc')
);

CREATE TABLE delivery_detail (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    orderid UUID NOT NULL,
    pickuppointid UUID NOT NULL,
    phonenumber VARCHAR(20) NOT NULL,
    firstname VARCHAR(100) NOT NULL,
    lastname VARCHAR(100) NOT NULL,
    middlename VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL CHECK (email LIKE '%@%'),
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

CREATE TABLE orders_reservations(
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    orderid uuid NOT NULL,
    clotheid uuid NOT NULL,
    colorid uuid NOT NULL,
    sizeid uuid NOT NULL,
    quantity INTEGER NOT NULL,
    reservedat TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    expiresat TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    isactive bool DEFAULT true,
    CONSTRAINT fk_orderid_order FOREIGN KEY (orderid)
        REFERENCES orders(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE INDEX idx_order_statusid ON orders(statusid);
CREATE INDEX idx_regions_ref ON regions(ref);
CREATE INDEX idx_settlements_ref ON settlements(ref);
CREATE INDEX idx_pickup_points_ref ON pickup_points(ref);
CREATE INDEX idx_order_userid ON orders(userid);
CREATE INDEX idx_orderitem_orderid ON order_item(orderid);
CREATE INDEX idx_settlements_regionid ON settlements(regionid);
CREATE INDEX idx_pickup_points_providerid ON pickup_points(deliveryproviderid);
CREATE INDEX idx_deliverydetail_orderid ON delivery_detail(orderid);
CREATE INDEX idx_deliverydetail_pickuppointsid ON delivery_detail(pickuppointid);
CREATE INDEX idx_orders_reservations_clothe_size_color_id ON orders_reservations(clotheid, colorid, sizeid);