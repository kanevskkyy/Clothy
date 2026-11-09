import {useEffect, useState} from "react";
import {Link, useOutletContext, useParams} from "react-router-dom";
import {ArrowLeft} from "lucide-react";
import {toast} from "sonner";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import {fraudApi, type IFraudTxRecord} from "../../../app/api/fraudApi.ts";
import {paymentApi} from "../../../app/api/paymentApi.ts";
import FraudRiskPanel from "../../../entities/fraud/FraudRiskPanel.tsx";
import OrderDetailPage from "../../account/orders/OrderDetailPage/OrderDetailPage.tsx";
import styles from "./AdminOrderDetailPage.module.css";

// Admin view of an order: the buyer's order page plus the fraud block, which buyers never see.
const AdminOrderDetailPage = () => {
    const {orderId} = useParams<{ orderId: string }>();
    const {setPageHeader} = useOutletContext<AdminLayoutContext>();

    // undefined = loading or fraud service unavailable, null = order not scored
    const [risk, setRisk] = useState<IFraudTxRecord | null | undefined>(undefined);
    const [paymentStatus, setPaymentStatus] = useState<string | undefined>(undefined);

    useEffect(() => {
        setPageHeader({title: "Order", description: "Order details and fraud risk"});
    }, []);

    useEffect(() => {
        if (!orderId) return;

        fraudApi.getOrderRiskAsync(orderId)
            .then(setRisk)
            .catch(() => {
                setRisk(undefined);
                toast.warning("Fraud check unavailable: order risk not shown");
            });

        paymentApi.getOrderPaymentStatusesAsync([orderId])
            .then((statuses) => setPaymentStatus(statuses[0]?.status))
            .catch(() => setPaymentStatus(undefined));
    }, [orderId]);

    return (
        <div className={styles.page}>
            <Link to="/admin/orders" className={styles.back}>
                <ArrowLeft size={16} aria-hidden="true"/> All orders
            </Link>

            <div className={styles.layout}>
                {/* the fraud panel renders nothing while loading or when the service is down */}
                <aside className={styles.aside} aria-label="Fraud risk">
                    <FraudRiskPanel record={risk} paymentStatus={paymentStatus}/>
                </aside>
                <div className={styles.detail}>
                    <OrderDetailPage/>
                </div>
            </div>
        </div>
    );
};

export default AdminOrderDetailPage;
