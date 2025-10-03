import "./widget.css";

type WidgetProps = {
    title?: string;
    footer?: string;
    children: React.ReactNode;
    minH?: number;
    onClick?: () => void;
    href?: string;
};

export default function Widget({ title, footer, children, minH = 120, onClick, href }: WidgetProps) {
    const hasHead = Boolean((title ?? "").trim() || footer);
    const interactive = Boolean(onClick || href);
    const className = `nx-widget${interactive ? " is-clickable" : ""}`;
    const bodyClass = `nx-widget-body${hasHead ? "" : " is-flush"}`;

    const content = (
        <>
            {hasHead && (
                <div className="nx-widget-head">
                    {title && <h4>{title}</h4>}
                    {footer && <span className="nx-widget-foot">{footer}</span>}
                </div>
            )}
            <div className={bodyClass}>{children}</div>
        </>
    );

    if (href) {
        return (
            <a className={className} style={{ minHeight: minH }} href={href}>
                {content}
            </a>
        );
    }

    if (onClick) {
        return (
            <button type="button" className={className} style={{ minHeight: minH }} onClick={onClick}>
                {content}
            </button>
        );
    }

    return (
        <section className={className} style={{ minHeight: minH }}>
            {content}
        </section>
    );
}
