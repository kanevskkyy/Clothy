import {useEffect, useState} from "react";
import {useOutletContext, useSearchParams} from "react-router-dom";
import {Helmet} from "react-helmet";
import {Package, ShieldAlert, ShieldCheck} from "lucide-react";
import {toast} from "sonner";
import styles from "./AdminOrdersPage.module.css";
import {getCurrentPage, handlePageChange} from "../../../shared/lib/paginationUtils";
import type {IOrderReadDTO} from "../../../entities/ordersService/interfaces/order/IOrderReadDTO";
import {ordersApi} from "../../../app/api/ordersApi";
import type {PagedList} from "../../../shared/lib/pagedList";
import {getErrorMessage} from "../../../shared/lib/errorHandler";
import SortSelect from "../../../features/catalog/sortSelect/SortSelect";
import {ORDER_STATUS_OPTIONS} from "../../../entities/ordersService/orderStatus";
import Pagination from "../../../shared/ui/Pagination/Pagination.tsx";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import {fraudApi, type IFraudRiskSummary} from "../../../app/api/fraudApi.ts";
import {paymentApi} from "../../../app/api/paymentApi.ts";
import RiskBadge from "../../../entities/fraud/RiskBadge.tsx";
import PaymentStatusBadge from "../../../entities/fraud/PaymentStatusBadge.tsx";
import AdminOrdersTable from "./AdminOrdersTable.tsx";
import AdminToolbar from "../ui/AdminToolbar.tsx";
import AdminToggle from "../ui/AdminToggle.tsx";
import AdminCard from "../ui/AdminCard.tsx";
import AdminEmptyState from "../ui/AdminEmptyState.tsx";
import AdminButton from "../ui/AdminButton.tsx";
import {AdminRowsSkeleton} from "../ui/AdminSkeleton.tsx";

// "on review" mode pages through the fraud decisions on the client
const REVIEW_PAGE_SIZE = 10;

type RiskMap = Record<string, IFraudRiskSummary | null>;

const AdminOrdersPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);
    const currentStatus = searchParams.get("status") || "";
    const onlyReview = searchParams.get("risk") === "review";
    const {setPageHeader} = useOutletContext<AdminLayoutContext>();

    const [isLoading, setIsLoading] = useState(true);
    const [orders, setOrders] = useState<PagedList<IOrderReadDTO> | null>(null);
    // undefined = fraud data not loaded (service unavailable): badges are hidden
    const [risks, setRisks] = useState<RiskMap | undefined>(undefined);
    const [paymentStatuses, setPaymentStatuses] = useState<Record<string, string>>({});

    useEffect(() => {
        setPageHeader({title: "Orders", description: "All store orders"});
    }, []);

    useEffect(() => {
        const loadPaymentStatusesAsync = async (orderIds: string[]) => {
            try {
                const statuses = await paymentApi.getOrderPaymentStatusesAsync(orderIds);
                setPaymentStatuses(Object.fromEntries(statuses.map((s) => [s.orderId, s.status])));
            } catch {
                setPaymentStatuses({});
            }
        };

        const loadRisksAsync = async (orderIds: string[]): Promise<RiskMap | undefined> => {
            const results = await Promise.allSettled(orderIds.map((id) => fraudApi.getOrderRiskAsync(id)));
            if (results.some((r) => r.status === "rejected")) {
                toast.warning("Fraud check unavailable: order risks not shown");
                return undefined;
            }
            return Object.fromEntries(orderIds.map((id, i) =>
                [id, (results[i] as PromiseFulfilledResult<IFraudRiskSummary | null>).value]));
        };

        const fetchOrdersAsync = async () => {
            const response = await ordersApi.getAllOrdersAsync(currentPage, currentStatus || undefined);
            const ids = response.items.map((o) => o.id);
            const [riskMap] = await Promise.all([loadRisksAsync(ids), loadPaymentStatusesAsync(ids)]);
            setOrders(response);
            setRisks(riskMap);
        };

        const fetchReviewOrdersAsync = async () => {
            let decisions;
            try {
                decisions = await fraudApi.getDecisionsAsync(true);
            } catch (error) {
                toast.error(`Fraud check unavailable: ${getErrorMessage(error)}`);
                setOrders(null);
                return;
            }

            const totalPages = Math.max(1, Math.ceil(decisions.length / REVIEW_PAGE_SIZE));
            const page = Math.min(currentPage, totalPages);
            const slice = decisions.slice((page - 1) * REVIEW_PAGE_SIZE, page * REVIEW_PAGE_SIZE);

            // decisions keep only the order id: load each order (deleted orders are skipped)
            const details = await Promise.allSettled(slice.map((d) => ordersApi.getOrderByIdAsync(d.txid, true)));
            const items: IOrderReadDTO[] = details.flatMap((r) => r.status === "fulfilled" ? [{
                id: r.value.id,
                status: r.value.status,
                userId: "",
                userFirstName: r.value.userFirstName,
                userLastName: r.value.userLastName,
                userEmail: r.value.userEmail,
                totalPrice: r.value.totalPrice,
                isFreeDelivery: r.value.isFreeDelivery,
                createdAt: r.value.createdAt,
                updatedAt: r.value.updatedAt,
            }] : []);

            await loadPaymentStatusesAsync(items.map((o) => o.id));
            setRisks(Object.fromEntries(slice.map((d) => [d.txid, d])));
            setOrders({
                currentPage: page,
                totalPages,
                pageSize: REVIEW_PAGE_SIZE,
                totalCount: decisions.length,
                hasPrevious: page > 1,
                hasNext: page < totalPages,
                items,
            });
        };

        const loadAsync = async () => {
            try {
                setIsLoading(true);
                if (onlyReview) await fetchReviewOrdersAsync();
                else await fetchOrdersAsync();
            } catch (error) {
                toast.error(getErrorMessage(error));
            } finally {
                setIsLoading(false);
            }
        };

        loadAsync();
    }, [currentPage, currentStatus, onlyReview]);

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

    const handleOnlyReviewChange = (checked: boolean) => {
        const params = new URLSearchParams(searchParams);
        if (checked) {
            params.set("risk", "review");
            // the fraud service does not know order statuses, the two filters are not combined
            params.delete("status");
        } else {
            params.delete("risk");
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

    const filters = (
        <AdminToolbar
            start={
                <>
                    <AdminToggle
                        pressed={onlyReview}
                        onChange={handleOnlyReviewChange}
                        icon={<ShieldAlert size={16}/>}
                    >
                        Needs review only
                    </AdminToggle>
                    {!isLoading && orders && (
                        <span className={styles.count} aria-live="polite">
                            {onlyReview
                                ? `Needs review: ${orders.totalCount}`
                                : `${orders.totalCount} orders`}
                        </span>
                    )}
                </>
            }
            end={!onlyReview ? (
                <SortSelect
                    value={currentStatus}
                    options={ORDER_STATUS_OPTIONS}
                    onChange={handleStatusChange}
                    label="Status:"
                />
            ) : (
                <span className={styles.hint}>Status filter is disabled in review mode</span>
            )}
        />
    );

    const renderBody = () => {
        if (isLoading) {
            return <AdminRowsSkeleton rows={8} columns={6} label="Loading orders"/>;
        }

        if (!orders || orders.items.length === 0) {
            return (
                <AdminEmptyState
                    icon={onlyReview ? <ShieldCheck size={22}/> : <Package size={22}/>}
                    title="No orders found"
                    description={onlyReview
                        ? "No orders flagged for review by the fraud check."
                        : "There are no orders matching the selected status."}
                    action={currentStatus || onlyReview ? (
                        <AdminButton
                            variant="secondary"
                            size="md"
                            onClick={() => onlyReview ? handleOnlyReviewChange(false) : handleStatusChange("")}
                        >
                            Clear Filter
                        </AdminButton>
                    ) : undefined}
                />
            );
        }

        return (
            <AdminOrdersTable
                orders={orders.items}
                onStatusChange={handleOrderStatusChange}
                getOrderLink={(order) => `/admin/orders/${order.id}`}
                renderPayment={(order) => (
                    <PaymentStatusBadge status={paymentStatuses[order.id]} disputesOnly/>
                )}
                renderRisk={risks ? (order) => <RiskBadge record={risks[order.id] ?? null}/> : undefined}
            />
        );
    };

    return (
        <div>
            <Helmet>
                <title>Admin — Orders</title>
                <meta name="description" content="Manage and review all customer orders."/>
            </Helmet>

            {filters}
            <AdminCard flush aria-label="Orders">
                {renderBody()}
            </AdminCard>
            {!isLoading && orders && orders.totalPages > 1 && (
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
