import {useEffect, useState} from "react";
import {useOutletContext, useSearchParams} from "react-router-dom";
import {Helmet} from "react-helmet";
import {Package} from "lucide-react";
import {toast} from "sonner";
import styles from "./AdminOrdersPage.module.css";
import {getCurrentPage, handlePageChange} from "../../../shared/lib/paginationUtils";
import type {IOrderReadDTO} from "../../../entities/ordersService/interfaces/order/IOrderReadDTO";
import {ordersApi} from "../../../app/api/ordersApi";
import type {PagedList} from "../../../shared/lib/pagedList";
import {getErrorMessage} from "../../../shared/lib/errorHandler";
import SortSelect from "../../../features/catalog/sortSelect/SortSelect";
import {ORDER_STATUS_OPTIONS} from "../../../entities/ordersService/orderStatus";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import EmptyState from "../../../shared/ui/EmptyState/EmptyState.tsx";
import OrderList from "../../../entities/ordersService/orderList/OrderList.tsx";
import Pagination from "../../../shared/ui/Pagination/Pagination.tsx";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";

const AdminOrdersPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);
    const currentStatus = searchParams.get("status") || "";
    const {setPageHeader} = useOutletContext<AdminLayoutContext>();

    const [isLoading, setIsLoading] = useState(true);
    const [orders, setOrders] = useState<PagedList<IOrderReadDTO> | null>(null);

    useEffect(() => {
        setPageHeader({title: "Orders", description: "All store orders"});
    }, []);

    useEffect(() => {
        const fetchOrdersAsync = async () => {
            try {
                setIsLoading(true);
                const response = await ordersApi.getAllOrdersAsync(currentPage, currentStatus || undefined);
                setOrders(response);
            } catch (error) {
                toast.error(getErrorMessage(error));
            } finally {
                setIsLoading(false);
            }
        };

        fetchOrdersAsync();
    }, [currentPage, currentStatus]);

    const handleStatusChange = (value: string) => {
        const params = new URLSearchParams(searchParams);
        if (value) {
            params.set("status", value);
        } else {
            params.delete("status");
        }
        params.delete("page");
        setSearchParams(params);
    };

    const handleOrderStatusChange = async (orderId: string, newStatus: string) => {
        try {
            await ordersApi.updateOrderAsync(orderId, newStatus);
            setOrders((prev) => {
                if (!prev) return prev;
                return {
                    ...prev,
                    items: prev.items.map((order) =>
                        order.id === orderId ? {...order, status: newStatus} : order
                    ),
                };
            });
            toast.success("Order status updated");
        } catch (error) {
            toast.error(getErrorMessage(error));
            throw error;
        }
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    if (isLoading) {
        return <Loader/>
    }

    if (!orders || orders.items.length === 0) {
        return (
            <EmptyState
                icon={<Package size={28} color="#6B6B6B"/>}
                title="No orders found"
                description="There are no orders matching the selected status."
                buttons={currentStatus ? [{
                    label: "Clear Filter",
                    onClick: () => handleStatusChange(""),
                    variant: "primary",
                    size: "md",
                }] : []}
            />
        );
    }

    return (
        <div>
            <Helmet>
                <title>Admin — Orders</title>
                <meta name="description" content="Manage and review all customer orders."/>
            </Helmet>

            <div className={styles.header}>
                <SortSelect
                    value={currentStatus}
                    options={ORDER_STATUS_OPTIONS}
                    onChange={handleStatusChange}
                    label="Status:"
                />
            </div>
            <OrderList orders={orders.items} onStatusChange={handleOrderStatusChange}/>
            {orders.totalPages > 1 && (
                <Pagination
                    currentPage={orders.currentPage}
                    totalPages={orders.totalPages}
                    onPageChange={onPageChange}
                />
            )}
        </div>
    );
};

export default AdminOrdersPage;