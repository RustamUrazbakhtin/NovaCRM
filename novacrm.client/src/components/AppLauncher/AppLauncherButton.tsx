import { forwardRef } from "react";
import styles from "./AppLauncher.module.css";

type Props = {
    id: string;
    isOpen: boolean;
    onToggle: () => void;
    controls: string;
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
                <svg viewBox="0 0 24 24" role="img" aria-hidden>
                    <rect x="3" y="3" width="4" height="4" rx="1.2" />
                    <rect x="10" y="3" width="4" height="4" rx="1.2" />
                    <rect x="17" y="3" width="4" height="4" rx="1.2" />
                    <rect x="3" y="10" width="4" height="4" rx="1.2" />
                    <rect x="10" y="10" width="4" height="4" rx="1.2" />
                    <rect x="17" y="10" width="4" height="4" rx="1.2" />
                    <rect x="3" y="17" width="4" height="4" rx="1.2" />
                    <rect x="10" y="17" width="4" height="4" rx="1.2" />
                    <rect x="17" y="17" width="4" height="4" rx="1.2" />
                </svg>
            </button>
        </div>
    );
});

export default AppLauncherButton;
