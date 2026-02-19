import {Home, ShieldAlert, ShoppingBasket} from 'lucide-react';
import ErrorPage from "../ErrorPage/ErrorPage.tsx";

const ForbiddenPage = () => {
    return (
        <ErrorPage
            title="Access Denied"
            message="You don't have permission to view this page. Please contact support if you believe this is an error."
            icon={ShieldAlert}
            pageTitle="Access Denied"
            iconColor="#DC2828"
            iconBgColor="#FBE9E9"
            actions={[
                {
                    label: 'Home',
                    href: '/',
                    icon: Home,
                    variant: 'primary'
                },
                {
                    label: 'To Catalog',
                    href: '/catalog',
                    icon: ShoppingBasket,
                    variant: 'secondary'
                }
            ]}
        />
    );
};

export default ForbiddenPage;