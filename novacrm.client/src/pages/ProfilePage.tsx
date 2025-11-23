import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Header from "../layout/Header";
import ThemeProvider from "../providers/ThemeProvider";
import { authApi } from "../app/auth";
import { getProfile, isAxiosValidationError, updateProfile, type Profile } from "../api/profile";
import "../styles/dashboard/index.css";
import "../styles/profile/profile.css";

type ProfileFormValues = {
    firstName: string;
    lastName: string;
    phone?: string | null;
    address?: string | null;
    notes?: string | null;
};

function normalizeNullable(value?: string | null) {
    return value ?? "";
}

export default function ProfilePage() {
    const navigate = useNavigate();
    const [profile, setProfile] = useState<Profile | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [serverError, setServerError] = useState<string | null>(null);
    const [toast, setToast] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        reset,
        watch,
        setError,
        formState: { errors, isDirty, isSubmitting, isValid },
    } = useForm<ProfileFormValues>({
        mode: "onChange",
        defaultValues: {
            firstName: "",
            lastName: "",
            phone: "",
            address: "",
            notes: "",
        },
    });

    const watchedValues = watch();

    useEffect(() => {
        const controller = new AbortController();
        setIsLoading(true);
        setServerError(null);

        getProfile(controller.signal)
            .then(result => {
                setProfile(result);
                reset({
                    firstName: result.firstName,
                    lastName: result.lastName,
                    phone: normalizeNullable(result.phone),
                    address: normalizeNullable(result.address),
                    notes: normalizeNullable(result.notes),
                }, { keepDirty: false });
            })
            .catch(err => {
                if (axios.isCancel(err)) return;
                if (axios.isAxiosError(err) && err.response?.status === 401) {
                    navigate("/auth", { replace: true });
                    return;
                }
                setServerError("Unable to load your profile right now. Please try again.");
            })
            .finally(() => setIsLoading(false));

        return () => controller.abort();
    }, [navigate, reset]);

    useEffect(() => {
        if (!toast) return undefined;
        const timeout = window.setTimeout(() => setToast(null), 4000);
        return () => window.clearTimeout(timeout);
    }, [toast]);

    const displayName = useMemo(() => {
        const first = watchedValues.firstName?.trim();
        const last = watchedValues.lastName?.trim();
        const parts = [first, last].filter(Boolean);
        if (parts.length) return parts.join(" ");
        if (profile) return `${profile.firstName} ${profile.lastName}`.trim();
        return "Your profile";
    }, [profile, watchedValues.firstName, watchedValues.lastName]);

    const handleLogout = () => {
        authApi.logout();
        navigate("/auth", { replace: true });
    };

    const onSubmit = handleSubmit(async values => {
        setServerError(null);
        setToast(null);
        try {
            const updated = await updateProfile({
                firstName: values.firstName.trim(),
                lastName: values.lastName.trim(),
                phone: values.phone?.trim() || null,
                address: values.address?.trim() || null,
                notes: values.notes?.trim() || null,
            });

            setProfile(updated);
            reset(
                {
                    firstName: updated.firstName,
                    lastName: updated.lastName,
                    phone: normalizeNullable(updated.phone),
                    address: normalizeNullable(updated.address),
                    notes: normalizeNullable(updated.notes),
                },
                { keepDirty: false }
            );
            setToast("Profile saved");
        } catch (err) {
            if (isAxiosValidationError(err)) {
                const responseErrors = err.response?.data?.errors;
                if (responseErrors) {
                    for (const [key, messages] of Object.entries(responseErrors)) {
                        if (Array.isArray(messages) && messages.length > 0) {
                            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
                            setError(camelKey as keyof ProfileFormValues, { type: "server", message: messages[0] });
                        }
                    }
                }

                if (err.response?.status === 401) {
                    navigate("/auth", { replace: true });
                    return;
                }

                setServerError(err.response?.data?.detail || err.response?.data?.message || "Unable to save profile.");
            } else {
                setServerError("Unable to save profile. Please try again.");
            }
        }
    });

    const handleCancel = () => {
        if (!profile) return;
        reset(
            {
                firstName: profile.firstName,
                lastName: profile.lastName,
                phone: normalizeNullable(profile.phone),
                address: normalizeNullable(profile.address),
                notes: normalizeNullable(profile.notes),
            },
            { keepDirty: false }
        );
        setServerError(null);
        setToast(null);
    };

    return (
        <ThemeProvider>
            <div className="profile-shell">
                <Header
                    breadcrumb="Profile"
                    onLogout={handleLogout}
                    userName={displayName || "Your profile"}
                    userEmail={profile?.email || ""}
                />
                <main className="profile-page" aria-labelledby="profile-title">
                    <section className="profile-sheet">
                        <form className="profile-form" onSubmit={onSubmit} noValidate>
                            <div className="profile-content">
                                <header className="profile-header">
                                    <div className="profile-meta">
                                        <h1 className="profile-title" id="profile-title">
                                            {displayName || "Your profile"}
                                        </h1>
                                        <p className="profile-subtitle">
                                            {profile?.email || "Update your personal details"}
                                        </p>
                                    </div>
                                </header>

                                {isLoading ? (
                                    <div className="profile-loading" aria-live="polite">
                                        Loading profile…
                                    </div>
                                ) : (
                                    <>
                                        <section className="profile-section" aria-labelledby="profile-section-primary">
                                            <div>
                                                <h2 className="profile-section-title" id="profile-section-primary">
                                                    Primary information
                                                </h2>
                                                <p className="profile-section-caption">
                                                    Keep your basic contact information up to date.
                                                </p>
                                            </div>
                                            <div className="profile-grid two-column">
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="firstName">
                                                        First name
                                                    </label>
                                                    <input
                                                        id="firstName"
                                                        className="profile-input"
                                                        aria-invalid={Boolean(errors.firstName)}
                                                        {...register("firstName", { required: "First name is required." })}
                                                        autoComplete="given-name"
                                                    />
                                                    {errors.firstName && (
                                                        <div className="profile-error">{errors.firstName.message}</div>
                                                    )}
                                                </div>
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="lastName">
                                                        Last name
                                                    </label>
                                                    <input
                                                        id="lastName"
                                                        className="profile-input"
                                                        aria-invalid={Boolean(errors.lastName)}
                                                        {...register("lastName", { required: "Last name is required." })}
                                                        autoComplete="family-name"
                                                    />
                                                    {errors.lastName && (
                                                        <div className="profile-error">{errors.lastName.message}</div>
                                                    )}
                                                </div>
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="email">
                                                        Email
                                                    </label>
                                                    <input
                                                        id="email"
                                                        className="profile-input"
                                                        value={profile?.email ?? ""}
                                                        readOnly
                                                        aria-readonly="true"
                                                    />
                                                    <div className="profile-hint">Email is managed by your administrator.</div>
                                                </div>
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="phone">
                                                        Phone
                                                    </label>
                                                    <input
                                                        id="phone"
                                                        className="profile-input"
                                                        aria-invalid={Boolean(errors.phone)}
                                                        {...register("phone")}
                                                        autoComplete="tel"
                                                    />
                                                    {errors.phone && <div className="profile-error">{errors.phone.message}</div>}
                                                </div>
                                            </div>
                                        </section>

                                        <section className="profile-section" aria-labelledby="profile-section-other">
                                            <div>
                                                <h2 className="profile-section-title" id="profile-section-other">
                                                    Other details
                                                </h2>
                                                <p className="profile-section-caption">
                                                    Share anything your teammates should know.
                                                </p>
                                            </div>
                                            <div className="profile-grid single-column">
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="address">
                                                        Address
                                                    </label>
                                                    <textarea
                                                        id="address"
                                                        className="profile-textarea"
                                                        rows={3}
                                                        aria-invalid={Boolean(errors.address)}
                                                        {...register("address")}
                                                    />
                                                    {errors.address && <div className="profile-error">{errors.address.message}</div>}
                                                </div>
                                                <div className="profile-field">
                                                    <label className="profile-label" htmlFor="notes">
                                                        Notes
                                                    </label>
                                                    <textarea
                                                        id="notes"
                                                        className="profile-textarea"
                                                        rows={4}
                                                        aria-invalid={Boolean(errors.notes)}
                                                        {...register("notes")}
                                                    />
                                                    {errors.notes && <div className="profile-error">{errors.notes.message}</div>}
                                                </div>
                                            </div>
                                        </section>
                                    </>
                                )}
                            </div>

                            <div className="profile-actions">
                                <div className="profile-status" aria-live="polite">
                                    {serverError && <div className="profile-message">{serverError}</div>}
                                    {!serverError && toast && (
                                        <div className="profile-toast" role="status">
                                            {toast}
                                        </div>
                                    )}
                                    {isDirty && !serverError && !toast && (
                                        <span className="profile-unsaved">Unsaved changes</span>
                                    )}
                                </div>
                                <div className="profile-buttons">
                                    <button
                                        type="button"
                                        className="profile-button secondary"
                                        onClick={handleCancel}
                                        disabled={isSubmitting || isLoading || !isDirty}
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="profile-button primary"
                                        disabled={isSubmitting || isLoading || !isValid || !isDirty}
                                    >
                                        {isSubmitting ? "Saving…" : "Save"}
                                    </button>
                                </div>
                            </div>
                        </form>
                    </section>
                </main>
            </div>
        </ThemeProvider>
    );
}
