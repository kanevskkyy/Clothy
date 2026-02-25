import { useEffect, useState } from "react";
import { adminApi, type IAdminDashboard } from "../../../app/api/adminApi";
import Loader from "../../../shared/ui/Loader/Loader";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler";
import { ShoppingBag, DollarSign, Clock, Package, Star, AlertTriangle } from "lucide-react";
import styles from "./AdminDashboardPage.module.css";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import {useOutletContext} from "react-router-dom";
import {Helmet} from "react-helmet";

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

    if (isLoading || !data) return <Loader />;

    const cards = [
        {
            title: "Orders today",
            value: data.newOrdersCount,
            label: "new orders",
            icon: <ShoppingBag size={18} />,
            iconClass: styles.iconGreen,
        },
        {
            title: "Revenue today",
            value: `$${data.totalPrice}`,
            label: "total amount",
            icon: <DollarSign size={18} />,
            iconClass: styles.iconGreen,
        },
        {
            title: "Awaiting processing",
            value: data.pendingOrdersCount,
            label: "pending orders",
            icon: <Clock size={18} />,
            iconClass: styles.iconOrange,
        },
        {
            title: "Goods in stock",
            value: data.totalItemsCount,
            label: "units",
            icon: <Package size={18} />,
            iconClass: styles.iconBlue,
        },
        {
            title: "Reviews under moderation",
            value: data.pendingReviewCount,
            label: "awaiting inspection",
            icon: <Star size={18} />,
            iconClass: styles.iconYellow,
        },
        {
            title: "Unanswered questions",
            value: data.questionsWithoutAnswerCount,
            label: "require attention",
            icon: <AlertTriangle size={18} />,
            iconClass: styles.iconRed,
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
                    <div className={styles.card} key={idx}>
                        <div className={styles.cardHeader}>
                            <span>{card.title}</span>
                            <div className={`${styles.cardIcon} ${card.iconClass}`}>{card.icon}</div>
                        </div>
                        <p className={styles.value}>{card.value}</p>
                        <p className={styles.label}>{card.label}</p>
                    </div>
                ))}
            </div>
        </>
    );
};

export default AdminDashboardPage;