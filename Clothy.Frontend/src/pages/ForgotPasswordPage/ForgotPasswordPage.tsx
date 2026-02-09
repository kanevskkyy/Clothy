import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import {Helmet} from "react-helmet";
import AuthLayout from "../../features/authLayout/AuthLayout.tsx";
import ForgotPasswordForm from "../../features/forgotPasswordForm/ForgotPasswordForm.tsx";
import styles from "./ForgotPasswordPage.module.css";

const ForgotPasswordPage = () => {
    return (
        <PageWrapper>
            <div className={styles.page}>
                <Helmet>
                    <title>Password recovery – Clothy account</title>
                    <meta name="description" content="Securely recover your Clothy account password. Enter your email to receive reset instructions" />
                </Helmet>

                <AuthLayout title="Forgot your password?" subtitle="Enter your email address and we will send you instructions for resetting your password">
                    <ForgotPasswordForm />
                </AuthLayout>
            </div>
        </PageWrapper>
    );
};

export default ForgotPasswordPage;