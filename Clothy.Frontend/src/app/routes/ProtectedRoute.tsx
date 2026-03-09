import { Navigate, Outlet } from "react-router-dom";
import {useAuthStore} from "../stores/authStore.ts";

const ProtectedRoute = () => {
    const isAuthenticated = useAuthStore(store => store.isAuthenticated);

    return isAuthenticated()
        ? <Outlet />
        : <Navigate to="/login" replace />;
};

export default ProtectedRoute;