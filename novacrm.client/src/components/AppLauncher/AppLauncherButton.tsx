import { forwardRef, type ReactNode } from "react";
import styles from "./AppLauncher.module.css";

type Props = {
    id: string;
    isOpen: boolean;
    onToggle: () => void;
    controls: string;
    icon?: ReactNode;
};

const defaultIcon = (
    <svg viewBox="0 0 20 20" role="img" aria-hidden="true">
        <circle cx="4" cy="4" r="1.4" fill="currentColor" />
        <circle cx="10" cy="4" r="1.4" fill="currentColor" />
        <circle cx="16" cy="4" r="1.4" fill="currentColor" />
        <circle cx="4" cy="10" r="1.4" fill="currentColor" />
        <circle cx="10" cy="10" r="1.4" fill="currentColor" />
        <circle cx="16" cy="10" r="1.4" fill="currentColor" />
        <circle cx="4" cy="16" r="1.4" fill="currentColor" />
        <circle cx="10" cy="16" r="1.4" fill="currentColor" />
        <circle cx="16" cy="16" r="1.4" fill="currentColor" />
    </svg>
);

const AppLauncherButton = forwardRef<HTMLButtonElement, Props>(function AppLauncherButton({
    id,
    isOpen,
    onToggle,
    controls,
    icon,
}, ref) {
    return (
        <div className={styles.buttonShell}>
            <button
                ref={ref}
                id={id}
                type="button"
                className={styles.trigger}
                aria-haspopup="dialog"
                aria-expanded={isOpen}
                aria-controls={controls}
                aria-label="Open app launcher"
                onClick={onToggle}
            >
                <span className={styles.srOnly}>Open app launcher</span>
                {icon ?? defaultIcon}
            </button>
        </div>
    );
});

export default AppLauncherButton;
