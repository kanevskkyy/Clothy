import { Outlet } from "react-router-dom";
import styles from "./AccountLayout.module.css";
import AccountHeader from "../accountHeader/AccountHeader.tsx";
import EmailVerificationBanner from "../emailBanner/EmailVerificationBanner.tsx";
import AccountSidebar from "../accountSidebar/AccountSidebar.tsx";
import type { IUserReadDTO } from "../../../entities/users/IUserReadDTO.ts";
import PageWrapper from "../../../shared/PageWrapper/PageWrapper.tsx";

const AccountLayout = () => {
    // TODO: Get this data from API, if user is not authorize redirect to login
    const user: IUserReadDTO = {
        id: "1",
        email: "gmae@gmail.com",
        firstName: "Elena",
        lastName: "Test",
        phoneNumber: "+380501234567",
        photoUrl: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=200&h=200&fit=crop",
        emailVerified: false,
    };

    return (
        <>
            <PageWrapper>
                <div className={styles.container}>
                    <EmailVerificationBanner emailVerified={user.emailVerified} />
                    <AccountHeader user={user} />
                    <div className={styles.content}>
                        <AccountSidebar />
                        <main className={styles.main}>
                            <Outlet context={{ user }} />
                        </main>
                    </div>
                </div>
            </PageWrapper>
        </>
    );
};

export default AccountLayout;