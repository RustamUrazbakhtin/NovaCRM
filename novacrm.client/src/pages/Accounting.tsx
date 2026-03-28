import PlaceholderTemplate from "./PlaceholderTemplate";

export default function AccountingPage() {
    return (
        <PlaceholderTemplate
            breadcrumb="Accounting"
            title="Accounting"
            description="Manage bookkeeping for US operations: revenue, payroll, expenses, forms, and pending accounting activity."
        >
            <div className="fx-placeholder-grid">
                <section>
                    <h3>Revenue summary</h3>
                    <p>Track posted revenue and monthly closing status.</p>
                </section>
                <section>
                    <h3>Payroll</h3>
                    <p>Payroll configuration is not set up yet.</p>
                </section>
                <section>
                    <h3>Expenses & payouts</h3>
                    <p>Expense ledgers and payout records will appear here.</p>
                </section>
                <section>
                    <h3>Forms & tax documents</h3>
                    <p>US tax forms are not configured yet.</p>
                </section>
                <section>
                    <h3>Pending / unpaid</h3>
                    <p>Outstanding accounting items will be shown here.</p>
                </section>
                <section>
                    <h3>Accounting activity</h3>
                    <p>Recent bookkeeping actions and reconciliations.</p>
                </section>
            </div>
        </PlaceholderTemplate>
    );
}
