import {useEffect, useState} from "react";
import {useOutletContext, useParams} from "react-router-dom";
import {toast} from "sonner";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import {fraudApi, type IFraudTxRecord} from "../../../app/api/fraudApi.ts";
import {paymentApi} from "../../../app/api/paymentApi.ts";
import FraudRiskPanel from "../../../entities/fraud/FraudRiskPanel.tsx";
import OrderDetailPage from "../../account/orders/OrderDetailPage/OrderDetailPage.tsx";

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
                toast.warning("Антифрод недоступний: ризик замовлення не показано");
            });

        paymentApi.getOrderPaymentStatusesAsync([orderId])
            .then((statuses) => setPaymentStatus(statuses[0]?.status))
            .catch(() => setPaymentStatus(undefined));
    }, [orderId]);

    return (
        <>
            <FraudRiskPanel record={risk} paymentStatus={paymentStatus}/>
            <OrderDetailPage/>
        </>
    );
};

export default AdminOrderDetailPage;
