import { forwardRef } from "react";
import styles from "./AppLauncher.module.css";

type Props = {
    id: string;
    isOpen: boolean;
    onToggle: () => void;
    controls: string;
    icon?: React.ReactNode;
};

const AppLauncherButton = forwardRef<HTMLButtonElement, Props>(function AppLauncherButton({
    id,
    isOpen,
    onToggle,
    controls,
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
                onClick={onToggle}
            >
                <span className={styles.srOnly}>Open app launcher</span>
                <svg viewBox="0 0 24 24" role="img" aria-hidden="true">
                    <circle cx="5.5" cy="5.5" r="1.9" fill="currentColor" />
                    <circle cx="12" cy="5.5" r="1.9" fill="currentColor" />
                    <circle cx="18.5" cy="5.5" r="1.9" fill="currentColor" />
                    <circle cx="5.5" cy="12" r="1.9" fill="currentColor" />
                    <circle cx="12" cy="12" r="1.9" fill="currentColor" />
                    <circle cx="18.5" cy="12" r="1.9" fill="currentColor" />
                    <circle cx="5.5" cy="18.5" r="1.9" fill="currentColor" />
                    <circle cx="12" cy="18.5" r="1.9" fill="currentColor" />
                    <circle cx="18.5" cy="18.5" r="1.9" fill="currentColor" />
                </svg>
            </button>
        </div>
    );
});

export default AppLauncherButton;
