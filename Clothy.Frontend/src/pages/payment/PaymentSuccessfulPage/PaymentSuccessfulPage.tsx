import styles from "./PaymentSuccessfulPage.module.css";
import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/authLayout/AuthLayout.tsx";
import PaymentSuccessful from "../../../features/checkout/paymantSuccessful/PaymentSuccessful.tsx";
import Container from "../../../shared/layout/Container/Container.tsx";

const PaymentSuccessfulPage = () => {
    return (
        <Container paddingY={10}>
            <div className={styles.page}>
                <Helmet>
                    <title>Payment Successful - Clothy</title>
                    <meta name="description" content="Your payment was successful"/>
                </Helmet>

                <AuthLayout
                    title="Payment successful!"
                    subtitle="Thank you for your order! We have already started processing it. You will receive a confirmation email shortly."
                >
                    <PaymentSuccessful/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default PaymentSuccessfulPage;