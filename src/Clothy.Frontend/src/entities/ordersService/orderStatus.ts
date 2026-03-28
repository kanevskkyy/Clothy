export const OrderStatus = {
    AwaitingPayment: "AwaitingPayment",
    ProcessingByManagers: "Processing",
    Shipped: "Shipped",
    Delivered: "Delivered",
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];

export const ORDER_STATUS_OPTIONS = [
    { value: "", label: "All Statuses" },
    { value: OrderStatus.AwaitingPayment, label: "AwaitingPayment" },
    { value: OrderStatus.ProcessingByManagers, label: "Processing" },
    { value: OrderStatus.Shipped, label: "Shipped" },
    { value: OrderStatus.Delivered, label: "Delivered" },
];