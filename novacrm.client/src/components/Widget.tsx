import "./widget.css";

type WidgetProps = {
    title?: string;
    footer?: string;
    children: React.ReactNode;
    minH?: number;
};

export default function Widget({ title, footer, children, minH = 140 }: WidgetProps) {
    const hasHead = Boolean((title ?? "").trim() || footer);

    return (
        <section className="nx-widget" style={{ minHeight: minH }}>
            {hasHead && (
                <div className="nx-widget-head">
                    {title && <h4>{title}</h4>}
                    {footer && <span className="nx-widget-foot">{footer}</span>}
                </div>
            )}
            <div className={`nx-widget-body${hasHead ? "" : " is-flush"}`}>{children}</div>
        </section>
    );
}
