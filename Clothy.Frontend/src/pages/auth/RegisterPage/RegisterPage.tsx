import styles from "../LoginPage/LoginPage.module.css";
import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/authLayout/AuthLayout.tsx";
import RegisterForm from "../../../features/forms/registerForm/RegisterForm.tsx";

const RegisterPage = () => {
    return (
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
    );
};

export default RegisterPage;