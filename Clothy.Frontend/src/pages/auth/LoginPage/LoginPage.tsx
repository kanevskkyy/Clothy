import { Helmet } from "react-helmet";
import styles from "./LoginPage.module.css";
import AuthLayout from "../../../features/auth/authLayout/AuthLayout.tsx";
import LoginForm from "../../../features/forms/loginForm/LoginForm.tsx";
import PageWrapper from "../../../shared/layout/PageWrapper/PageWrapper.tsx";

const LoginPage = () => {
    return (
        <PageWrapper>
            <div className={styles.page}>
                <Helmet>
                    <title>Log In - Clothy</title>
                    <meta name="description" content="Log in to your Clothy account" />
                </Helmet>

                <AuthLayout title="Log in" subtitle="Log in to your account">
                    <LoginForm />
                </AuthLayout>
            </div>
        </PageWrapper>
    );
};

export default LoginPage;