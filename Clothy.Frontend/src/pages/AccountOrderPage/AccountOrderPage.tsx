import type { IOrderReadDTO } from "../../entities/orders/order/IOrderReadDTO";
import OrderList from "../../entities/orders/order/orderList/OrderList.tsx";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import { useSearchParams } from "react-router-dom";
import type { PagedList } from "../../shared/pagedList.ts";
import { Helmet } from "react-helmet";
import {getCurrentPage, handlePageChange} from "../../shared/paginationUtils.ts";

const AccountOrderPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);

    const mockPagedOrders: PagedList<IOrderReadDTO> = {
        currentPage: currentPage,
        totalPages: 2,
        pageSize: 5,
        totalCount: 8,
        hasPrevious: currentPage > 1,
        hasNext: currentPage < 2,
        items: [
            {
                id: "a3f1c9e2-5b7d-4a91-9f2a-1c2d3e4f5a61",
                status: {
                    id: "11111111-1111-1111-1111-111111111111",
                    name: "Pending",
                    createdAt: "2026-02-01T10:00:00Z",
                },
                userId: "b1d2c3a4-6e7f-48a1-9b2c-0d1e2f3a4b5c",
                userFirstName: "John",
                userLastName: "Doe",
                userEmail: "john.doe@example.com",
                totalPrice: 120.5,
                isFreeDelivery: false,
                createdAt: "2026-02-01T10:05:00Z",
                updatedAt: "2026-02-01T10:10:00Z",
            },
            {
                id: "c4b2a190-8d7e-4c6b-9a1f-2e3d4c5b6a70",
                status: {
                    id: "22222222-2222-2222-2222-222222222222",
                    name: "Paid",
                    createdAt: "2026-02-02T09:00:00Z",
                },
                userId: "d2e3f4a5-b6c7-4890-9d1e-2f3a4b5c6d7e",
                userFirstName: "Anna",
                userLastName: "Smith",
                userEmail: "anna.smith@example.com",
                totalPrice: 89.99,
                isFreeDelivery: true,
                createdAt: "2026-02-02T09:15:00Z",
            },
            {
                id: "e5a1b2c3-d4f6-47a8-91ab-2cd3ef4a5b6c",
                status: {
                    id: "33333333-3333-3333-3333-333333333333",
                    name: "Shipped",
                    createdAt: "2026-02-03T08:00:00Z",
                },
                userId: "f1e2d3c4-b5a6-4789-8c1d-0e2f3a4b5c6d",
                userFirstName: "Michael",
                userLastName: "Brown",
                userEmail: "michael.brown@example.com",
                totalPrice: 250,
                isFreeDelivery: true,
                createdAt: "2026-02-03T08:20:00Z",
            },
            {
                id: "0f1e2d3c-4b5a-4978-9c1d-2e3f4a5b6c7d",
                status: {
                    id: "44444444-4444-4444-4444-444444444444",
                    name: "Delivered",
                    createdAt: "2026-02-04T12:00:00Z",
                    updatedAt: "2026-02-05T14:00:00Z",
                },
                userId: "9a8b7c6d-5e4f-4321-9a0b-1c2d3e4f5a6b",
                userFirstName: "Emily",
                userLastName: "Johnson",
                userEmail: "emily.johnson@example.com",
                totalPrice: 59.9,
                isFreeDelivery: false,
                createdAt: "2026-02-04T12:10:00Z",
            },
        ],
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    //#TODO: Connect to API and fetch paged orders based on currentPage

    return (
        <div>
            <Helmet>
                <title>My Orders | Account</title>
                <meta
                    name="description"
                    content="View and track your orders, delivery status, and order history in your account."
                />
            </Helmet>

            <OrderList orders={mockPagedOrders.items} />

            <Pagination
                currentPage={mockPagedOrders.currentPage}
                totalPages={mockPagedOrders.totalPages}
                onPageChange={onPageChange}
            />
        </div>
    );
};

export default AccountOrderPage;