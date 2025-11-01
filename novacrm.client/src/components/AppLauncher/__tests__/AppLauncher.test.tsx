import { describe, expect, it, beforeEach, vi, afterEach } from "vitest";
import { render, screen, fireEvent, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, useLocation } from "react-router-dom";
import { AppLauncher } from "../../AppLauncher";

const createItems = () => [
    { id: "calendar", label: "Calendar", icon: <span />, href: "/" },
    { id: "clients", label: "Clients", icon: <span />, href: "/clients" },
    { id: "settings", label: "Settings", icon: <span />, onSelect: vi.fn() },
];

function LocationEcho() {
    const location = useLocation();
    return <div data-testid="location">{location.pathname}</div>;
}

function renderLauncher(items = createItems()) {
    const utils = render(
        <MemoryRouter initialEntries={["/start"]}>
            <LocationEcho />
            <AppLauncher items={items} idPrefix="launcher" />
        </MemoryRouter>
    );

    return {
        ...utils,
        items,
        trigger: () => screen.getByRole("button", { name: /open app launcher/i }),
        getDialog: () => screen.getByRole("dialog"),
    };
}

const sanitize = (html: string) =>
    html
        .replace(/AppLauncher_module__[a-zA-Z0-9_-]+/g, "CLASS")
        .replace(/id="[^"]+"/g, 'id="ID"')
        .replace(/aria-controls="[^"]+"/g, 'aria-controls="ID-panel"');

let originalRaf: typeof window.requestAnimationFrame;

beforeEach(() => {
    vi.spyOn(window, "alert").mockImplementation(() => {});
    Object.defineProperty(window, "innerWidth", { value: 1024, configurable: true });
    Object.defineProperty(document.documentElement, "clientWidth", { value: 1024, configurable: true });
    originalRaf = window.requestAnimationFrame;
    vi.spyOn(window, "requestAnimationFrame").mockImplementation(cb => setTimeout(cb, 0));
    vi.spyOn(window, "cancelAnimationFrame").mockImplementation(id => clearTimeout(id as number));
});

afterEach(() => {
    window.requestAnimationFrame = originalRaf;
    vi.restoreAllMocks();
});

describe("AppLauncher interactions", () => {
    it("opens and closes via escape", async () => {
        const user = userEvent.setup();
        const { trigger } = renderLauncher();

        await user.click(trigger());
        expect(screen.getByRole("dialog")).toBeInTheDocument();

        await user.keyboard("{Escape}");

        expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
        expect(trigger()).toHaveFocus();
    });

    it("closes when clicking the overlay", async () => {
        const user = userEvent.setup();
        const { trigger } = renderLauncher();

        await user.click(trigger());
        const overlay = screen.getByRole("dialog").parentElement!;
        fireEvent.mouseDown(overlay);
        expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });

    it("supports keyboard navigation and activating items", async () => {
        const user = userEvent.setup();
        const { trigger } = renderLauncher();

        await user.click(trigger());
        await screen.findByRole("dialog");
        const grid = screen.getByRole("grid");
        const buttons = within(grid).getAllByRole("gridcell");
        expect(buttons[0]).toHaveFocus();

        await user.keyboard("{ArrowRight}");
        expect(buttons[1]).toHaveFocus();

        await user.keyboard("{End}");
        expect(buttons[2]).toHaveFocus();

        await user.keyboard("{Enter}");
        expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });

    it("navigates via href and closes", async () => {
        const user = userEvent.setup();
        const { trigger, items } = renderLauncher();

        await user.click(trigger());
        await screen.findByRole("dialog");
        const grid = screen.getByRole("grid");
        const buttons = within(grid).getAllByRole("gridcell");

        await user.keyboard("{ArrowRight}");
        expect(buttons[1]).toHaveFocus();

        await user.keyboard("{Enter}");

        expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
        expect(screen.getByTestId("location")).toHaveTextContent("/clients");

        // last item handler still available
        await user.click(trigger());
        await user.keyboard("{End}");
        await user.keyboard("{Enter}");
        const handler = items[2].onSelect as ReturnType<typeof vi.fn>;
        expect(handler).toHaveBeenCalled();
    });
});

describe("AppLauncher snapshots", () => {
    it("matches light mode snapshot", () => {
        const { container } = render(
            <MemoryRouter>
                <AppLauncher items={createItems()} idPrefix="snap" />
            </MemoryRouter>
        );
        expect(sanitize(container.innerHTML)).toMatchInlineSnapshot(`
            "<div class=\"CLASS\"><button id=\"ID\" type=\"button\" class=\"CLASS\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"ID-panel\"><span class=\"CLASS\">Open app launcher</span><svg viewBox=\"0 0 24 24\" role=\"img\" aria-hidden=\"true\"><circle cx=\"5.5\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"5.5\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"5.5\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle></svg></button></div>"
        `);
    });

    it("matches dark mode snapshot", () => {
        document.documentElement.classList.add("dark");
        const { container } = render(
            <MemoryRouter>
                <AppLauncher items={createItems()} idPrefix="snap-dark" />
            </MemoryRouter>
        );
        expect(sanitize(container.innerHTML)).toMatchInlineSnapshot(`
            "<div class=\"CLASS\"><button id=\"ID\" type=\"button\" class=\"CLASS\" aria-haspopup=\"dialog\" aria-expanded=\"false\" aria-controls=\"ID-panel\"><span class=\"CLASS\">Open app launcher</span><svg viewBox=\"0 0 24 24\" role=\"img\" aria-hidden=\"true\"><circle cx=\"5.5\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"5.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"5.5\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"12\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"5.5\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"12\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle><circle cx=\"18.5\" cy=\"18.5\" r=\"1.9\" fill=\"currentColor\"></circle></svg></button></div>"
        `);
        document.documentElement.classList.remove("dark");
    });
});
