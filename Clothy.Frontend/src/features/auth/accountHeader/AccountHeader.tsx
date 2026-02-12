import { LogOut } from "lucide-react";
import styles from "./AccountHeader.module.css";
import Button from "../../../shared/Button/Button.tsx";
import type { IUserReadDTO } from "../../../entities/users/IUserReadDTO.ts";

interface AccountHeaderProps {
    user: IUserReadDTO;
}

const AccountHeader = ({ user }: AccountHeaderProps) => {
    const handleLogout = () => {
        // TODO: Connect to API
        console.log("Logout clicked");
    };

    return (
        <div className={styles.header}>
            <div className={styles.userInfo}>
                <img
                    src={user.photoUrl}
                    alt={`${user.firstName} ${user.lastName}`}
                    className={styles.avatar}
                />
                <div className={styles.details}>
                    <h1 className={styles.name}>
                        {user.firstName} {user.lastName}
                    </h1>
                    <p className={styles.email}>{user.email}</p>
                </div>
            </div>
            <Button
                onClick={handleLogout}
                variant="outline"
                size="sm"
                icon={<LogOut size={18} />}
            >
                Logout
            </Button>
        </div>
    );
};

export default AccountHeader;