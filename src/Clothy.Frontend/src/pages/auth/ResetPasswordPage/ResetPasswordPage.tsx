import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/user/authLayout/AuthLayout.tsx";
import ResetPasswordForm from "../../../features/forms/resetPasswordForm/ResetPasswordForm.tsx";
import styles from "./ResetPassword.module.css";
import Container from "../../../shared/layout/Container/Container.tsx";

const ResetPasswordPage = () => {
    return (
        <Container paddingY={0}>
            <div className={styles.page}>
                <Helmet>
                    <title>Reset password - Clothy</title>
                    <meta name="description" content="Reset your password in Clothy"/>
                </Helmet>

                <AuthLayout title="Reset password" subtitle="Reset your password">
                    <ResetPasswordForm/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default ResetPasswordPage;