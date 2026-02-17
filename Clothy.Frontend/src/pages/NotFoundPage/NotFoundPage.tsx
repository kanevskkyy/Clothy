import ErrorPage from '../ErrorPage/ErrorPage';
import {Home, SearchIcon, ShoppingBasket} from "lucide-react";

const NotFoundPage = () => {
    return (
        <ErrorPage
            title="Page Not Found"
            message="It seems the page got lost along the way. No worries — we still have plenty of great products for you!"
            icon={SearchIcon}
            pageTitle="Page Not Found"
            iconColor="#6B6B6B"
            iconBgColor="#E1E1E1"
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

export default NotFoundPage;