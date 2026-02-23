import styles from "../LoginPage/LoginPage.module.css";
import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/authLayout/AuthLayout.tsx";
import RegisterForm from "../../../features/forms/registerForm/RegisterForm.tsx";
import Container from "../../../shared/layout/Container/Container.tsx";

const RegisterPage = () => {
    return (
        <Container paddingY={0}>
            <div className={styles.page}>
                <Helmet>
                    <title>Register - Clothy</title>
                    <meta name="description" content="Register your account in Clothy"/>
                </Helmet>

                <div className={styles.wrapper}>
                    <AuthLayout title="Register" subtitle="Create your account">
                        <RegisterForm/>
                    </AuthLayout>
                </div>
            </div>
        </Container>
    );
};

export default RegisterPage;