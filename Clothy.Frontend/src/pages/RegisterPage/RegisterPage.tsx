import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import styles from "../LoginPage/LoginPage.module.css";
import {Helmet} from "react-helmet";
import AuthLayout from "../../features/authLayout/AuthLayout.tsx";
import RegisterForm from "../../features/registerForm/RegisterForm.tsx";

const RegisterPage = () => {
    return (
        <PageWrapper>
            <div className={styles.page}>
                <Helmet>
                    <title>Register - Clothy</title>
                    <meta name="description" content="Register your account in Clothy" />
                </Helmet>

                <div className={styles.wrapper}>
                    <AuthLayout title="Register" subtitle="Create your account">
                        <RegisterForm />
                    </AuthLayout>
                </div>
            </div>
        </PageWrapper>
    );
};

export default RegisterPage;