import { Outlet } from "react-router-dom";
import styles from "./UserLayout.module.css";
import Header from "../../../features/layout/header/Header.tsx";
import Footer from "../../../features/layout/footer/Footer.tsx";

const UserLayout = () => {
    return (
        <div className={styles.appWrapper}>
            <Header />
            <main className={styles.mainContent}>
                <Outlet />
            </main>
            <Footer />
        </div>
    );
};

export default UserLayout;