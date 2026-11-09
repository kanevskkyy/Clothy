import React, {useState} from "react";
import {Link} from "react-router-dom";
import {Check, Clock, Loader as LoaderIcon, Truck} from "lucide-react";
import type {IOrderReadDTO} from "../../../entities/ordersService/interfaces/order/IOrderReadDTO.ts";
import {formatDate} from "../../../shared/lib/formatDate.ts";
import Select from "../../../shared/ui/Select/Select.tsx";
import AdminTable from "../ui/AdminTable.tsx";
import AdminBadge, {type AdminBadgeTone} from "../ui/AdminBadge.tsx";
import table from "../ui/AdminTable.module.css";
import styles from "./AdminOrdersPage.module.css";

// same options and behaviour as the buyer OrderList in admin mode
const ADMIN_STATUS_OPTIONS = [
    {value: "Shipped", label: "Shipped"},
    {value: "Delivered", label: "Delivered"},
];

const STATUS_TONES: Record<string, { tone: AdminBadgeTone; icon: React.ReactNode }> = {
    AwaitingPayment: {tone: "neutral", icon: <Clock size={12}/>},
    Processing: {tone: "warning", icon: <LoaderIcon size={12}/>},
    Shipped: {tone: "info", icon: <Truck size={12}/>},
    Delivered: {tone: "success", icon: <Check size={12}/>},
};

interface AdminOrdersTableProps {
    orders: IOrderReadDTO[];
    onStatusChange: (orderId: string, newStatus: string) => Promise<void>;
    getOrderLink: (order: IOrderReadDTO) => string;
    /** dispute status of the payment */
    renderPayment?: (order: IOrderReadDTO) => React.ReactNode;
    /** fraud risk badge; the column is hidden when not given */
    renderRisk?: (order: IOrderReadDTO) => React.ReactNode;
}

const shortId = (id: string) => id.length > 8 ? id.slice(0, 8) : id;

const AdminOrdersTable: React.FC<AdminOrdersTableProps> = ({orders, onStatusChange, getOrderLink, renderPayment, renderRisk}) => {
    const [loadingId, setLoadingId] = useState<string | null>(null);

    const handleStatusChange = async (orderId: string, newStatus: string) => {
        setLoadingId(orderId);
        try {
            await onStatusChange(orderId, newStatus);
        } finally {
            setLoadingId(null);
        }
    };

    return (
        <AdminTable caption="Orders">
            <thead>
            <tr>
                <th scope="col">Order</th>
                <th scope="col">Customer</th>
                <th scope="col">Status</th>
                <th scope="col">Payment</th>
                {renderRisk && <th scope="col">Risk</th>}
                <th scope="col" className={table.num}>Total</th>
                <th scope="col"><span className={table.srOnly}>Change status</span></th>
            </tr>
            </thead>
            <tbody>
            {orders.map((order) => {
                const status = STATUS_TONES[order.status] ?? {tone: "neutral" as const, icon: null};
                const customer = `${order.userFirstName ?? ""} ${order.userLastName ?? ""}`.trim();
                return (
                    <tr key={order.id}>
                        <td className={table.nowrap}>
                            <div className={styles.stack}>
                                <Link
                                    to={getOrderLink(order)}
                                    className={`${table.link} ${table.mono}`}
                                    aria-label={`Open order ${order.id}`}
                                    title={order.id}
                                >
                                    #{shortId(order.id)}
                                </Link>
                                <span className={table.muted}>{formatDate(order.createdAt)}</span>
                            </div>
                        </td>
                        <td>
                            <div className={`${styles.stack} ${styles.customer}`}>
                                <span className={table.strong}>{customer || "—"}</span>
                                {order.userEmail && <span className={table.muted}>{order.userEmail}</span>}
                            </div>
                        </td>
                        <td className={table.nowrap}>
                            <AdminBadge tone={status.tone} icon={status.icon}>{order.status}</AdminBadge>
                        </td>
                        <td className={table.nowrap}>
                            {renderPayment?.(order) ?? null}
                        </td>
                        {renderRisk && <td className={table.nowrap}>{renderRisk(order)}</td>}
                        <td className={`${table.num} ${table.strong}`}>{order.totalPrice} $</td>
                        <td className={table.actions}>
                            <div className={styles.statusSelect}>
                                <Select
                                    aria-label={`Change status of order ${order.id}`}
                                    options={ADMIN_STATUS_OPTIONS}
                                    value={ADMIN_STATUS_OPTIONS.find(o => o.value === order.status) ?? null}
                                    onChange={(option) => {
                                        if (option) {
                                            handleStatusChange(order.id, (option as {value: string}).value);
                                        }
                                    }}
                                    isDisabled={loadingId === order.id}
                                    isLoading={loadingId === order.id}
                                    placeholder="Set status..."
                                    menuPortalTarget={document.body}
                                    menuPosition="fixed"
                                    styles={{
                                        menuPortal: (base) => ({...base, zIndex: 9999}),
                                        control: (base) => ({...base, minHeight: "36px", fontSize: "13px", borderRadius: "6px"}),
                                        valueContainer: (base) => ({...base, padding: "0 10px"}),
                                        indicatorsContainer: (base) => ({...base, height: "34px"}),
                                        option: (base) => ({...base, fontSize: "13px"}),
                                    }}
                                />
                            </div>
                        </td>
                    </tr>
                );
            })}
            </tbody>
        </AdminTable>
    );
};

export default AdminOrdersTable;
