import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "../stores/authStore.ts";

const AdminRoute = () => {
    const isAuthenticated = useAuthStore(store => store.isAuthenticated);
    const user = useAuthStore(store => store.user);

    if (!isAuthenticated()) {
        return <Navigate to="/login" replace />;
    }

    const isAdmin = user?.roles?.includes("Admin");

    if (!isAdmin) {
        return <Navigate to="not found" replace />;
    }

    return <Outlet />;
};

export default AdminRoute;