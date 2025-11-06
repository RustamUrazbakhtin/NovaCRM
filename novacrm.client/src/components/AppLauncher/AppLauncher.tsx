import { useCallback, useEffect, useId, useMemo, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import AppLauncherButton from "./AppLauncherButton";
import AppLauncherPanel from "./AppLauncherPanel";
import type { AppLauncherItemConfig } from "./types";

export type { AppLauncherItemConfig } from "./types";

type Props = {
    items: AppLauncherItemConfig[];
    onOpenChange?: (open: boolean) => void;
    idPrefix?: string;
    triggerIcon?: React.ReactNode; 

};

export default function AppLauncher({ items, onOpenChange, idPrefix }: Props) {
    const [open, setOpen] = useState(false);
    const triggerRef = useRef<HTMLButtonElement>(null);
    const navigate = useNavigate();
    const location = useLocation();
    const triggerId = idPrefix ?? useId();

    useEffect(() => {
        setOpen(false);
    }, [location.key]);

    useEffect(() => {
        onOpenChange?.(open);
    }, [open, onOpenChange]);

    const handleToggle = () => setOpen(prev => !prev);
    const handleClose = useCallback(() => {
        setOpen(false);
    }, []);

    const enhancedItems = useMemo(
        () =>
            items.map(item => ({
                ...item,
                onSelect: () => {
                    if (item.href) navigate(item.href);
                    item.onSelect?.();
                },
            })),
        [items, navigate],
    );

    return (
        <>
            <AppLauncherButton
                ref={triggerRef}
                id={triggerId}
                isOpen={open}
                onToggle={handleToggle}
                controls={`${triggerId}-panel`}
            />
            <AppLauncherPanel
                isOpen={open}
                onClose={handleClose}
                triggerId={triggerId}
                triggerRef={triggerRef}
                items={enhancedItems}
            />
        </>
    );
}
