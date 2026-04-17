import { describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import StaffPage from "./Staff";

const { createStaffMock, updateStaffMock, getStaffByIdMock } = vi.hoisted(() => ({
  createStaffMock: vi.fn(),
  updateStaffMock: vi.fn(),
  getStaffByIdMock: vi.fn(),
}));

vi.mock("../api/staff", () => ({
  getStaffList: vi.fn(async () => ({ overview: { totalStaff: 1, activeToday: 1, bookedToday: 1, availableToday: 0, avgRating: 4.8, payrollThisMonth: 5000 }, items: [] })),
  getStaffCatalog: vi.fn(async () => ({ roles: [], specializations: [], branches: [], users: [] })),
  getStaffById: getStaffByIdMock,
  createStaff: createStaffMock,
  updateStaff: updateStaffMock,
}));

describe("StaffPage", () => {
  it("renders KPI cards", async () => {
    render(<MemoryRouter><StaffPage /></MemoryRouter>);
    await waitFor(() => expect(screen.getByRole("heading", { name: "Staff" })).toBeInTheDocument());
    expect(screen.getByText("Total Staff")).toBeInTheDocument();
    expect(screen.getByText("Payroll")).toBeInTheDocument();
  });

  it("sends deterministic create payload and clears non-selected compensation fields", async () => {
    createStaffMock.mockResolvedValueOnce({});
    render(<MemoryRouter><StaffPage /></MemoryRouter>);
    fireEvent.click((await screen.findAllByRole("button", { name: /Add Staff/i }))[0]);
    fireEvent.change(screen.getByLabelText("First name"), { target: { value: "Jane" } });
    fireEvent.change(screen.getByLabelText("Last name"), { target: { value: "Doe" } });
    fireEvent.change(screen.getByLabelText("Status"), { target: { value: "Vacation" } });
    fireEvent.click(screen.getByRole("switch"));
    fireEvent.change(screen.getByLabelText("Compensation type"), { target: { value: "HourlyRate" } });
    fireEvent.change(screen.getByLabelText("Hourly rate"), { target: { value: "35" } });
    fireEvent.click(screen.getByRole("button", { name: "Save" }));

    await waitFor(() => expect(createStaffMock).toHaveBeenCalledTimes(1));
    expect(createStaffMock.mock.calls[0][0]).toMatchObject({
      firstName: "Jane",
      lastName: "Doe",
      employmentStatus: "Vacation",
      compensationType: "HourlyRate",
      hourlyRate: 35,
      fixedSalary: null,
      commissionPercent: null,
      hasCrmAccess: true,
    });
  });
});
