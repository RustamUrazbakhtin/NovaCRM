import { useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useTheme } from "../providers/ThemeProvider";
import { AppLauncher, type AppLauncherItemConfig } from "../components/AppLauncher";
import "./header.css";

export default function Header({
    breadcrumb = "Dashboard",
    onLogout,
    userName = "User",
    userEmail = "user@example.com",
}: {
    breadcrumb?: string;
    onLogout: () => void;
    userName?: string;
    userEmail?: string;
}) {
    const navigate = useNavigate();
    const { theme, setTheme, isDark } = useTheme();
    const [open, setOpen] = useState(false);
    const box = useRef<HTMLDivElement>(null);
    const menuButtonRef = useRef<HTMLButtonElement>(null);
    const icon = (paths: ReactNode) => (
        <svg viewBox="0 0 24 24" width="32" height="32" role="img" aria-hidden>
            <g
                fill="none"
                stroke="currentColor"
                strokeWidth={1.6}
                strokeLinecap="round"
                strokeLinejoin="round"
            >
                {paths}
            </g>
        </svg>
    );

    const launcherItems: AppLauncherItemConfig[] = useMemo(
        () => [
            {
                id: "calendar",
                label: "Calendar",
                icon: icon(
                    <>
                        <rect x="4" y="5" width="16" height="14" rx="3" />
                        <line x1="4" y1="9" x2="20" y2="9" />
                        <line x1="8" y1="3" x2="8" y2="7" />
                        <line x1="16" y1="3" x2="16" y2="7" />
                    </>
                ),
                href: "/calendar",
            },
            {
                id: "clients",
                label: "Clients",
                icon: icon(
                    <>
                        <circle cx="8" cy="9" r="3" />
                        <circle cx="16" cy="9" r="3" />
                        <path d="M3.5 18.5c.8-2.5 2.8-4.5 4.5-4.5s3.7 2 4.5 4.5" />
                        <path d="M12.5 18.5c.8-2.5 2.8-4.5 4.5-4.5s3.7 2 4.5 4.5" />
                    </>
                ),
                href: "/clients",
            },
            {
                id: "staff",
                label: "Staff",
                icon: icon(
                    <>
                        <path d="M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4Z" />
                        <path d="M5 21a7 7 0 0 1 14 0" />
                    </>
                ),
                href: "/staff",
            },
            {
                id: "inventory",
                label: "Inventory",
                icon: icon(
                    <>
                        <rect x="4" y="4" width="16" height="16" rx="2.5" />
                        <path d="M4 10h16" />
                        <path d="M10 4v6" />
                    </>
                ),
                href: "/inventory",
            },
            {
                id: "analytics",
                label: "Analytics",
                icon: icon(
                    <>
                        <path d="M4 20h16" />
                        <rect x="5.5" y="10" width="3" height="6.5" rx="1.2" />
                        <rect x="10.5" y="7" width="3" height="9.5" rx="1.2" />
                        <rect x="15.5" y="12" width="3" height="4.5" rx="1.2" />
                    </>
                ),
                href: "/analytics",
            },
            {
                id: "tasks",
                label: "Tasks",
                icon: icon(
                    <>
                        <path d="M8 4h11a1 1 0 0 1 1 1v11" />
                        <path d="M16 20H5a1 1 0 0 1-1-1V8" />
                        <path d="m9 11 2 2 4-4" />
                    </>
                ),
                href: "/tasks",
            },
            {
                id: "reviews",
                label: "Reviews",
                icon: icon(
                    <>
                        <path d="M6 4h12a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2h-6l-4 3v-3H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z" />
                        <path d="M9 9h6" />
                        <path d="M9 12h4" />
                    </>
                ),
                href: "/reviews",
            },
            {
                id: "settings",
                label: "Settings",
                icon: icon(
                    <>
                        <circle cx="12" cy="12" r="3" />
                        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09a1.65 1.65 0 0 0 1.51-1 1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1Z" />
                    </>
                ),
                href: "/settings",
            },
        ],
        [icon],
    );

    useEffect(() => {
        const onDoc = (e: MouseEvent) => {
            if (!box.current?.contains(e.target as Node)) setOpen(false);
        };
        document.addEventListener("click", onDoc);
        return () => document.removeEventListener("click", onDoc);
    }, []);

    useEffect(() => {
        if (!open) return undefined;

        const onKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                event.preventDefault();
                setOpen(false);
            }
        };

        document.addEventListener("keydown", onKeyDown);
        return () => document.removeEventListener("keydown", onKeyDown);
    }, [open]);

    const wasOpen = useRef(false);

    useEffect(() => {
        if (wasOpen.current && !open) {
            menuButtonRef.current?.focus();
        }
        wasOpen.current = open;
    }, [open]);

    const isSwitchOn = theme === "dark" || (theme === "system" && isDark);
    const handleThemeToggle = () => {
        if (theme === "system") {
            setTheme(isSwitchOn ? "light" : "dark");
            return;
        }

        setTheme(isSwitchOn ? "light" : "dark");
    };

    const accountActions = useMemo(
        () => [
            { label: "Company profile", to: "/settings/company" },
            { label: "Subscription & billing", to: "/settings/billing" },
        ],
        []
    );

    const userInitials = useMemo(() => {
        const letters = userName
            .trim()
            .split(/\s+/)
            .filter(Boolean)
            .map(part => part.charAt(0).toUpperCase());

        if (letters.length === 0) {
            return "U";
        }

        return letters.slice(0, 2).join("");
    }, [userName]);

    const handleNavigate = (to: string) => {
        navigate(to);
        setOpen(false);
    };

    return (
        <header className="nx-header">
            <div className="nx-left">
                <div className="nx-logo">
                    <span className="nx-mark">
                        <img src="/NovaCRM_Logo.png" alt="NovaCRM" height={28} />
                    </span>
                    <span className="brand__name">NovaCRM</span>
                </div>
                <div className="nx-divider" />
                <div className="nx-breadcrumb">{breadcrumb}</div>
            </div>

            <div className="nx-right" ref={box}>
                <AppLauncher
                    items={launcherItems}
                    onOpenChange={next => {
                        if (next) setOpen(false);
                    }}
                />
                <button
                    ref={menuButtonRef}
                    className="nx-user"
                    onClick={() => setOpen(v => !v)}
                    aria-haspopup="menu"
                    aria-expanded={open}
                    aria-controls={open ? "nx-user-menu" : undefined}
                >
                    <img src="/user.svg" alt="user avatar" />
                    <span className="nx-user-name">User</span>
                </button>

                {open && (
                    <div className="nx-menu" role="menu" id="nx-user-menu" aria-label="User menu">
                        <div className="nx-menu-section" role="none">
                            <button
                                type="button"
                                className="nx-account-card"
                                role="menuitem"
                                onClick={() => handleNavigate("/settings/profile")}
                            >
                                <span className="nx-account-avatar" aria-hidden="true">
                                    {userInitials}
                                </span>
                                <span className="nx-account-meta">
                                    <span className="nx-account-name">{userName || "User"}</span>
                                    <span className="nx-account-email">{userEmail || "user@example.com"}</span>
                                    <span className="nx-account-caption">Manage your account</span>
                                </span>
                            </button>
                        </div>

                        <div className="nx-menu-section" role="none">
                            <span className="nx-menu-section-title" aria-hidden="true">
                                Account
                            </span>
                            <div className="nx-menu-group" role="none">
                                {accountActions.map(action => (
                                    <button
                                        key={action.to}
                                        type="button"
                                        className="nx-menu-item"
                                        role="menuitem"
                                        onClick={() => handleNavigate(action.to)}
                                    >
                                        {action.label}
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div className="nx-menu-section" role="group" aria-label="Theme preferences">
                            <div className="nx-theme-row">
                                <div className="nx-theme-info">
                                    <span className="nx-theme-title">
                                        {theme === "light"
                                            ? "Light mode"
                                            : theme === "dark"
                                                ? "Dark mode"
                                                : "System theme"}
                                    </span>
                                    <span className="nx-theme-subtitle">Theme</span>
                                </div>

                                <button
                                    type="button"
                                    className={`nx-switch ${isSwitchOn ? "is-on" : ""}`}
                                    role="switch"
                                    aria-checked={isSwitchOn}
                                    aria-label={isSwitchOn ? "Switch to light theme" : "Switch to dark theme"}
                                    onClick={handleThemeToggle}
                                >
                                    <span className="nx-switch-thumb" />
                                </button>
                            </div>

                            <button
                                type="button"
                                className={`nx-theme-system ${theme === "system" ? "is-active" : ""}`}
                                aria-pressed={theme === "system"}
                                onClick={() => {
                                    setTheme("system");
                                    setOpen(false);
                                }}
                            >
                                Follow system settings
                            </button>
                        </div>

                        <div className="nx-menu-divider" role="separator" aria-hidden="true" />

                        <div className="nx-menu-section" role="none">
                            <button
                                type="button"
                                className="nx-menu-signout"
                                role="menuitem"
                                onClick={() => {
                                    setOpen(false);
                                    onLogout();
                                }}
                            >
                                Sign out
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </header>
    );
}