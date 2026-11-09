import { useEffect, useState } from "react";
import { adminApi, type IAdminDashboard } from "../../../app/api/adminApi";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler";
import { ShoppingBag, DollarSign, Clock, Package, Star, AlertTriangle } from "lucide-react";
import styles from "./AdminDashboardPage.module.css";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import {useOutletContext} from "react-router-dom";
import {Helmet} from "react-helmet";
import {AdminTilesSkeleton} from "../ui/AdminSkeleton.tsx";

const AdminDashboardPage = () => {
    const [data, setData] = useState<IAdminDashboard | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();

    const fetchDashboard = async () => {
        try {
            setIsLoading(true);
            const result = await adminApi.getDashboardAsync();
            setData(result);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchDashboard();
        setPageHeader({ title: "Dashboard", description: "Today's store review" });
    }, []);

    if (isLoading || !data) return <AdminTilesSkeleton count={6} height={128} label="Loading dashboard"/>;

    const cards = [
        {
            title: "Orders today",
            value: data.newOrdersCount,
            label: "new orders",
            icon: <ShoppingBag size={16} />,
            attention: false,
        },
        {
            title: "Revenue today",
            value: `$${data.totalPrice}`,
            label: "total amount",
            icon: <DollarSign size={16} />,
            attention: false,
        },
        {
            title: "Awaiting processing",
            value: data.pendingOrdersCount,
            label: "processing orders",
            icon: <Clock size={16} />,
            attention: data.pendingOrdersCount > 0,
        },
        {
            title: "Goods in stock",
            value: data.totalItemsCount,
            label: "units",
            icon: <Package size={16} />,
            attention: false,
        },
        {
            title: "Reviews under moderation",
            value: data.pendingReviewCount,
            label: "awaiting inspection",
            icon: <Star size={16} />,
            attention: data.pendingReviewCount > 0,
        },
        {
            title: "Unanswered questions",
            value: data.questionsWithoutAnswerCount,
            label: "require attention",
            icon: <AlertTriangle size={16} />,
            attention: data.questionsWithoutAnswerCount > 0,
        },
    ];

    return (
        <>
            <Helmet>
                <title>Admin — Dashboard</title>
                <meta name="description" content="Get statistics in admin panel"/>
            </Helmet>

            <div className={styles.grid}>
                {cards.map((card, idx) => (
                    <section
                        className={`${styles.card} ${card.attention ? styles.attention : ""}`}
                        key={idx}
                        aria-label={card.title}
                    >
                        <div className={styles.cardHeader}>
                            <h2 className={styles.title}>{card.title}</h2>
                            <div className={styles.cardIcon} aria-hidden="true">{card.icon}</div>
                        </div>
                        <p className={styles.value}>{card.value}</p>
                        <p className={styles.label}>{card.label}</p>
                    </section>
                ))}
            </div>
        </>
    );
};

export default AdminDashboardPage;
