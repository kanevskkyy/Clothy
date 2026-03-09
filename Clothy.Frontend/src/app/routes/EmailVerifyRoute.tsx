import {Navigate, Outlet} from "react-router-dom";
import {useAuthStore} from "../stores/authStore.ts";
import {decodeJwt} from "../../shared/lib/decodeJwt.ts";

const EmailVerifyRoute = () => {
    const accessToken = useAuthStore(store => store.accessToken);

    if (!accessToken) return <Navigate to="/login" replace/>;

    const decodedToken = decodeJwt(accessToken);
    if (decodedToken.email_verified) return <Navigate to="/" replace/>;

    return <Outlet/>;
};

export default EmailVerifyRoute;