import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import styles from "./PaymentSuccessfulPage.module.css";
import { Helmet } from "react-helmet";
import AuthLayout from "../../features/authLayout/AuthLayout.tsx";
import PaymentSuccessful from "../../features/paymantSuccessful/PaymentSuccessful.tsx";

const PaymentSuccessfulPage = () => {
    return (
        <PageWrapper>
            <div className={styles.page}>
                <Helmet>
                    <title>Payment Successful - Clothy</title>
                    <meta name="description" content="Your payment was successful" />
                </Helmet>

                <AuthLayout
                    title="Payment successful!"
                    subtitle="Thank you for your order! We have already started processing it. You will receive a confirmation email shortly."
                >
                    <PaymentSuccessful />
                </AuthLayout>
            </div>
        </PageWrapper>
    );
};

export default PaymentSuccessfulPage;