import { Navigate, Outlet } from "react-router-dom";
import {useAuthStore} from "../stores/authStore.ts";

const GuestRoute = () => {
    const isAuthenticated = useAuthStore(store => store.isAuthenticated);

    return isAuthenticated()
        ? <Navigate to="/account" replace />
        : <Outlet />;
};

export default GuestRoute;