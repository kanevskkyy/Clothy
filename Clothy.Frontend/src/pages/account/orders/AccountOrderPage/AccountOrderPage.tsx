import type { IOrderReadDTO } from "../../../../entities/ordersService/order/IOrderReadDTO.ts";
import OrderList from "../../../../entities/ordersService/order/orderList/OrderList.tsx";
import Pagination from "../../../../shared/ui/Pagination/Pagination.tsx";
import { useSearchParams } from "react-router-dom";
import type { PagedList } from "../../../../shared/lib/pagedList.ts";
import { Helmet } from "react-helmet";
import { getCurrentPage, handlePageChange } from "../../../../shared/lib/paginationUtils.ts";
import { useEffect, useState } from "react";
import { ordersApi } from "../../../../app/api/ordersApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import EmptyState from "../../../../shared/ui/EmptyState/EmptyState.tsx";
import {Package} from "lucide-react";

const AccountOrderPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);

    const [isLoading, setIsLoading] = useState(true); // Змінено на true для першого завантаження
    const [myOrders, setMyOrders] = useState<PagedList<IOrderReadDTO> | null>(null);

    useEffect(() => {
        const fetchMyOrdersAsync = async () => {
            try {
                setIsLoading(true);
                const response = await ordersApi.getMyOrdersAsync(currentPage);
                setMyOrders(response);
            } catch (error) {
                toast.error(getErrorMessage(error));
            } finally {
                setIsLoading(false);
            }
        };

        fetchMyOrdersAsync();
    }, [currentPage]);

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    if (isLoading) {
        return <Loader />;
    }

    if (!myOrders || myOrders.items.length === 0) {
        return (
            <EmptyState
                icon={<Package size={28} color="#6B6B6B" />}
                title="No orders yet"
                description="Once you place an order, it will appear here. Browse our catalog and find something special for yourself."
                buttons={[
                    {
                        label: "Go to catalog",
                        to: "/catalog",
                        variant: "primary",
                        size: "md",
                        fullWidth: false
                    }
                ]}
            />
        );
    }

    return (
        <div>
            <Helmet>
                <title>My Orders | Account</title>
                <meta
                    name="description"
                    content="View and track your orders, delivery status, and order history in your account."
                />
            </Helmet>

            <OrderList orders={myOrders.items} />

            {myOrders.totalPages > 1 && (
                <Pagination
                    currentPage={myOrders.currentPage}
                    totalPages={myOrders.totalPages}
                    onPageChange={onPageChange}
                />
            )}
        </div>
    );
};

export default AccountOrderPage;