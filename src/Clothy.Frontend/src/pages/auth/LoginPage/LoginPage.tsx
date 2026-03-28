import {Helmet} from "react-helmet";
import styles from "./LoginPage.module.css";
import AuthLayout from "../../../features/auth/user/authLayout/AuthLayout.tsx";
import LoginForm from "../../../features/forms/loginForm/LoginForm.tsx";
import Container from "../../../shared/layout/Container/Container.tsx";

const LoginPage = () => {
    return (
        <Container paddingY={0}>
            <div className={styles.page}>
                <Helmet>
                    <title>Log In - Clothy</title>
                    <meta name="description" content="Log in to your Clothy account"/>
                </Helmet>

                <AuthLayout title="Log in" subtitle="Log in to your account">
                    <LoginForm/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default LoginPage;