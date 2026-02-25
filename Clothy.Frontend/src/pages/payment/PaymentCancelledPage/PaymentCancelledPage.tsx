import styles from "./PaymentCancelledPage.module.css";
import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/user/authLayout/AuthLayout.tsx";
import PaymentCancelled from "../../../features/checkout/paymentCancelled/PaymentCancelled.tsx";
import {Navigate, useSearchParams} from "react-router-dom";
import Container from "../../../shared/layout/Container/Container.tsx";

const PaymentCancelledPage = () => {
    const [searchParams] = useSearchParams();
    const paymentId = searchParams.get("paymentId");

    if (!paymentId) {
        return <Navigate to="/not-found" replace/>;
    }

    return (
        <Container paddingY={10}>
            <div className={styles.page}>
                <Helmet>
                    <title>Payment Cancelled - Clothy</title>
                    <meta name="description" content="Your payment was cancelled"/>
                </Helmet>

                <AuthLayout
                    title="Payment canceled!"
                    subtitle="Unfortunately, your payment was not completed. This may have been due to a canceled transaction or technical issues. Your items are still in your cart."
                >
                    <PaymentCancelled paymentId={paymentId}/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default PaymentCancelledPage;