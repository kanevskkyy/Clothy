import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/user/authLayout/AuthLayout.tsx";
import ForgotPasswordForm from "../../../features/forms/forgotPasswordForm/ForgotPasswordForm.tsx";
import styles from "./ForgotPasswordPage.module.css";
import Container from "../../../shared/layout/Container/Container.tsx";

const ForgotPasswordPage = () => {
    return (
        <Container paddingY={0}>
            <div className={styles.page}>
                <Helmet>
                    <title>Password recovery – Clothy account</title>
                    <meta name="description"
                          content="Securely recover your Clothy account password. Enter your email to receive reset instructions"/>
                </Helmet>

                <AuthLayout title="Forgot your password?"
                            subtitle="Enter your email address and we will send you instructions for resetting your password">
                    <ForgotPasswordForm/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default ForgotPasswordPage;