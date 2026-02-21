import styles from './ErrorPage.module.css';
import { Helmet } from "react-helmet";
import PageWrapper from "../../../shared/layout/PageWrapper/PageWrapper.tsx";
import type {LucideIcon} from "lucide-react";

interface ActionButton {
    label: string;
    href: string;
    icon: LucideIcon;
    variant: 'primary' | 'secondary';
}

interface ErrorPageProps {
    title: string;
    message: string;
    icon: LucideIcon;
    iconColor: string;
    iconBgColor: string;
    pageTitle?: string;
    actions: ActionButton[];
}

const ErrorPage = ({ title, message, icon: Icon,  iconColor, iconBgColor, pageTitle, actions }: ErrorPageProps) => {
    return (
        <PageWrapper>
            <div className={styles.wrapper}>
                <Helmet>
                    <title>{pageTitle || title} | Clothy</title>
                </Helmet>

                <div className={styles.container}>
                    <div className={styles.mainIcon} style={{backgroundColor: iconBgColor}}>
                        <Icon size={24} style={{ color: iconColor }} />
                    </div>
                    <h1>{title}</h1>
                    <p>{message}</p>
                    <div className={styles.actions}>
                        {actions.map((action, index) => (
                            <a
                                key={index}
                                href={action.href}
                                className={
                                    action.variant === 'primary'
                                        ? styles.primaryButton
                                        : styles.secondaryButton
                                }
                            >
                                <action.icon size={20} />
                                {action.label}
                            </a>
                        ))}
                    </div>
                </div>
            </div>
        </PageWrapper>
    );
};

export default ErrorPage;