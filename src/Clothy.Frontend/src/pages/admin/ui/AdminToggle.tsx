import type {ReactNode} from "react";
import {Check} from "lucide-react";
import styles from "./AdminToggle.module.css";

interface AdminToggleProps {
    pressed: boolean;
    onChange: (pressed: boolean) => void;
    icon?: ReactNode;
    children: ReactNode;
}

/** On/off filter chip. Keeps the checkbox contract: onChange gets the new value. */
const AdminToggle = ({pressed, onChange, icon, children}: AdminToggleProps) => (
    <button
        type="button"
        aria-pressed={pressed}
        className={`${styles.toggle} ${pressed ? styles.on : ""}`}
        onClick={() => onChange(!pressed)}
    >
        <span className={styles.box} aria-hidden="true"><Check size={12} strokeWidth={3}/></span>
        {icon && <span aria-hidden="true" style={{display: "inline-flex"}}>{icon}</span>}
        {children}
    </button>
);

export default AdminToggle;
