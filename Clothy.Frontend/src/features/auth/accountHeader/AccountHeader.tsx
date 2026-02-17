import { LogOut } from "lucide-react";
import styles from "./AccountHeader.module.css";
import Button from "../../../shared/Button/Button.tsx";
import type { IUserReadDTO } from "../../../entities/usersService/IUserReadDTO.ts";
import { authApi } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";
import { getErrorMessage } from "../../../shared/utils/errorHandler.ts";

interface AccountHeaderProps {
    user: IUserReadDTO;
}

const AccountHeader = ({ user }: AccountHeaderProps) => {
    const navigate = useNavigate();

    const handleLogout = async () => {
        try {
            await authApi.logoutAsync();
            toast.success("Successfully logged out");
            navigate("/login", { replace: true });
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
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