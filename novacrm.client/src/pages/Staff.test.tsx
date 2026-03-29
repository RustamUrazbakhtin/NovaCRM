import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import StaffPage from "./Staff";

vi.mock("../api/staff", () => ({
  getStaffList: vi.fn(async () => ({ overview: { totalStaff: 1, activeToday: 1, bookedToday: 1, availableToday: 0, avgRating: 4.8, payrollThisMonth: 5000 }, items: [] })),
  getStaffCatalog: vi.fn(async () => ({ roles: [], specializations: [], branches: [], users: [] })),
  createStaff: vi.fn(),
  updateStaff: vi.fn(),
}));

describe("StaffPage", () => {
  it("renders KPI cards", async () => {
    render(<MemoryRouter><StaffPage /></MemoryRouter>);
    await waitFor(() => expect(screen.getByText("Staff")).toBeInTheDocument());
    expect(screen.getByText("Total Staff")).toBeInTheDocument();
    expect(screen.getByText("Payroll")).toBeInTheDocument();
  });
});
