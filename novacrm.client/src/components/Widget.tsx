import "./widget.css";

type Props = {
    title: string;
    footer?: string;
    children: React.ReactNode;
    minH?: number;
    className?: string;
};

export default function Widget({ title, footer, children, minH = 140, className }: Props) {
    const classes = ["nx-widget", className].filter(Boolean).join(" ");

    return (
        <section className={classes} style={{ minHeight: minH }}>
            <div className="nx-widget-head">
                <h4>{title}</h4>
                {footer && <span className="nx-widget-foot">{footer}</span>}
            </div>
            <div className="nx-widget-body">{children}</div>
        </section>
    );
}
