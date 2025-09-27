import "./widget.css";

export default function Widget({
    title, footer, children, minH = 140,
}: { title: string; footer?: string; children: React.ReactNode; minH?: number; }) {
    return (
        <section className="nx-widget" style={{ minHeight: minH }}>
            <div className="nx-widget-head">
                <h4>{title}</h4>
                {footer && <span className="nx-widget-foot">{footer}</span>}
            </div>
            <div className="nx-widget-body">{children}</div>
        </section>
    );
}
