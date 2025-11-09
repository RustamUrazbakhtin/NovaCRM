import axios from "axios";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { authApi } from "../app/auth";
import { TextInput } from "../components/TextInput";
import Button from "../components/Button";

const countries = [
    "United States",
    "Canada",
    "United Kingdom",
    "Germany",
    "France",
    "Italy",
    "Spain",
    "Australia",
    "Other"
];

const timezones = [
    "America/New_York",
    "America/Chicago",
    "America/Denver",
    "America/Los_Angeles",
    "Europe/London",
    "Europe/Berlin"
];

const phoneCodes = [
    { label: "+1 (US)", value: "+1" },
    { label: "+1 (CA)", value: "+1" },
    { label: "+44 (UK)", value: "+44" },
    { label: "+49 (DE)", value: "+49" },
    { label: "+33 (FR)", value: "+33" },
    { label: "+39 (IT)", value: "+39" },
    { label: "+34 (ES)", value: "+34" },
    { label: "+61 (AU)", value: "+61" }
];

const signInSchema = z.object({
    email: z.string().trim().email("Enter a valid email"),
    password: z.string().min(6, "Min 6 characters")
});

const signUpSchema = z
    .object({
        ownerEmail: z.string().trim().email("Enter a valid email"),
        ownerPassword: z.string().min(6, "Min 6 characters"),
        ownerPasswordRepeat: z.string().min(6, "Min 6 characters"),
        companyName: z.string().trim().min(1, "Required"),
        country: z.string().min(1, "Required"),
        timezone: z.string().min(1, "Required"),
        phoneCountryCode: z.string().min(1, "Required"),
        phoneNumber: z.string().trim().min(4, "Required"),
        businessId: z.string().trim().optional()
    })
    .refine((data) => data.ownerPassword === data.ownerPasswordRepeat, {
        path: ["ownerPasswordRepeat"],
        message: "Passwords must match"
    });

type SignInForm = z.infer<typeof signInSchema>;
type SignUpForm = z.infer<typeof signUpSchema>;
type PlanType = "free" | "pro" | "enterprise";

const plans: Array<{ value: PlanType; title: string; price: string; description: string }> = [
    { value: "free", title: "Free", price: "$0 / month", description: "Basic features" },
    { value: "pro", title: "Pro", price: "$39 / month", description: "For growing businesses" },
    { value: "enterprise", title: "Enterprise", price: "Contact us", description: "Custom solutions" }
];

const defaultSignUpValues: SignUpForm = {
    ownerEmail: "",
    ownerPassword: "",
    ownerPasswordRepeat: "",
    companyName: "",
    country: countries[0],
    timezone: timezones[0],
    phoneCountryCode: phoneCodes[0].value,
    phoneNumber: "",
    businessId: ""
};

export default function AuthPage() {
    const [tab, setTab] = useState<"sign-in" | "sign-up">("sign-in");
    const [planType, setPlanType] = useState<PlanType>("free");

    const signInForm = useForm<SignInForm>({ resolver: zodResolver(signInSchema) });
    const signUpForm = useForm<SignUpForm>({
        resolver: zodResolver(signUpSchema),
        defaultValues: defaultSignUpValues
    });

    const handleSignIn = signInForm.handleSubmit(async data => {
        await authApi.login(data.email, data.password);
        location.href = "/";
    });

    const handleSignUp = signUpForm.handleSubmit(async data => {
        signUpForm.clearErrors("root");
        try {
            const companyPhone = `${data.phoneCountryCode} ${data.phoneNumber.trim()}`.trim();

            const payload = {
                companyName: data.companyName.trim(),
                country: data.country,
                timezone: data.timezone,
                companyPhone,
                ownerEmail: data.ownerEmail.trim(),
                ownerPassword: data.ownerPassword,
                ownerPasswordRepeat: data.ownerPasswordRepeat,
                businessId: data.businessId?.trim() || undefined
            };

            await authApi.register(payload);

            const ownerEmail = payload.ownerEmail;
            signUpForm.reset({ ...defaultSignUpValues });
            signInForm.reset({ email: ownerEmail, password: "" });
            setPlanType("free");
            setTab("sign-in");
            alert("Account created. Please sign in.");
        } catch (error) {
            if (axios.isAxiosError(error) && error.response?.status === 400 && error.response.data?.errors) {
                const fieldErrors = error.response.data.errors as Record<string, string[]>;
                signUpForm.clearErrors();
                const general: string[] = [];
                const values = signUpForm.getValues() as Record<string, unknown>;

                const mapServerField = (key: string) => {
                    const normalized = key
                        .replace(/^\$\./, "")
                        .replace(/^registrationRequest\./i, "");

                    const camelKey = normalized.length > 0
                        ? normalized[0].toLowerCase() + normalized.slice(1)
                        : normalized;

                    if (camelKey === "companyPhone") {
                        return "phoneNumber";
                    }

                    return camelKey;
                };

                Object.entries(fieldErrors).forEach(([key, messages]) => {
                    const message = messages.join(" ");
                    const targetKey = mapServerField(key);

                    if (targetKey && targetKey in values) {
                        signUpForm.setError(targetKey as keyof SignUpForm, { type: "server", message });
                    } else {
                        general.push(message);
                    }
                });

                if (general.length) {
                    signUpForm.setError("root", { type: "server", message: general.join(" ") });
                }
            } else {
                signUpForm.setError("root", { type: "server", message: "Registration failed. Please try again." });
            }
        }
    });

    const showSignIn = () => {
        setTab("sign-in");
        signUpForm.clearErrors();
    };

    const showSignUp = () => {
        setTab("sign-up");
        signInForm.clearErrors();
    };

    return (
        <div className="centered">
            <div className="panel overflow-y-auto max-h-[90vh]">
                <div className="stack" style={{ gap: 16 }}>
                    <div className="tabs">
                        <button
                            type="button"
                            className={`tab ${tab === "sign-in" ? "active" : ""}`}
                            onClick={showSignIn}
                        >
                            Sign In
                        </button>
                        <button
                            type="button"
                            className={`tab ${tab === "sign-up" ? "active" : ""}`}
                            onClick={showSignUp}
                        >
                            Create Account
                        </button>
                    </div>

                    <form
                        className="stack"
                        style={{ gap: 16 }}
                        onSubmit={tab === "sign-in" ? handleSignIn : handleSignUp}
                    >
                        {tab === "sign-in" ? (
                            <>
                                <TextInput
                                    type="email"
                                    placeholder="Email"
                                    {...signInForm.register("email")}
                                    error={signInForm.formState.errors.email?.message}
                                />
                                <TextInput
                                    type="password"
                                    placeholder="Password"
                                    {...signInForm.register("password")}
                                    error={signInForm.formState.errors.password?.message}
                                />
                            </>
                        ) : (
                            <div className="stack" style={{ gap: 20 }}>
                                <div className="stack" style={{ gap: 12 }}>
                                    <div className="helper" style={{ textTransform: "uppercase", letterSpacing: 0.5 }}>
                                        Account info
                                    </div>
                                    <TextInput
                                        type="email"
                                        placeholder="Email"
                                        {...signUpForm.register("ownerEmail")}
                                        error={signUpForm.formState.errors.ownerEmail?.message}
                                    />
                                    <TextInput
                                        type="password"
                                        placeholder="Password"
                                        {...signUpForm.register("ownerPassword")}
                                        error={signUpForm.formState.errors.ownerPassword?.message}
                                    />
                                    <TextInput
                                        type="password"
                                        placeholder="Repeat password"
                                        {...signUpForm.register("ownerPasswordRepeat")}
                                        error={signUpForm.formState.errors.ownerPasswordRepeat?.message}
                                    />
                                </div>

                                <div className="stack" style={{ gap: 12 }}>
                                    <div className="helper" style={{ textTransform: "uppercase", letterSpacing: 0.5 }}>
                                        Company info
                                    </div>
                                    <TextInput
                                        placeholder="Company name"
                                        {...signUpForm.register("companyName")}
                                        error={signUpForm.formState.errors.companyName?.message}
                                    />
                                    <div className="stack">
                                        <select
                                            className="input"
                                            {...signUpForm.register("country")}
                                        >
                                            {countries.map(country => (
                                                <option key={country} value={country}>
                                                    {country}
                                                </option>
                                            ))}
                                        </select>
                                        {signUpForm.formState.errors.country?.message && (
                                            <div className="helper">{signUpForm.formState.errors.country.message}</div>
                                        )}
                                    </div>
                                    <div className="stack">
                                        <select
                                            className="input"
                                            {...signUpForm.register("timezone")}
                                        >
                                            {timezones.map(tz => (
                                                <option key={tz} value={tz}>
                                                    {tz}
                                                </option>
                                            ))}
                                        </select>
                                        {signUpForm.formState.errors.timezone?.message && (
                                            <div className="helper">{signUpForm.formState.errors.timezone.message}</div>
                                        )}
                                    </div>
                                    <div className="stack">
                                        <label className="helper" style={{ textTransform: "uppercase", letterSpacing: 0.5 }}>
                                            Company phone
                                        </label>
                                        <div className="flex gap-2">
                                            <select
                                                className="input w-28"
                                                {...signUpForm.register("phoneCountryCode")}
                                            >
                                                {phoneCodes.map(code => (
                                                    <option key={`${code.label}-${code.value}`} value={code.value}>
                                                        {code.label}
                                                    </option>
                                                ))}
                                            </select>
                                            <TextInput
                                                placeholder="Phone number"
                                                {...signUpForm.register("phoneNumber")}
                                                error={signUpForm.formState.errors.phoneNumber?.message}
                                            />
                                        </div>
                                    </div>
                                    <TextInput
                                        placeholder="Tax ID / Registration number (optional)"
                                        {...signUpForm.register("businessId")}
                                        error={signUpForm.formState.errors.businessId?.message}
                                    />
                                </div>

                                <div className="stack" style={{ gap: 12 }}>
                                    <div className="helper" style={{ textTransform: "uppercase", letterSpacing: 0.5 }}>
                                        Subscription plan
                                    </div>
                                    <div className="stack" style={{ gap: 8 }}>
                                        {plans.map(plan => (
                                            <label
                                                key={plan.value}
                                                className={`flex cursor-pointer items-start gap-3 rounded border px-3 py-2 ${
                                                    planType === plan.value ? "border-sky-500" : "border-slate-200"
                                                }`}
                                            >
                                                <input
                                                    type="radio"
                                                    name="plan"
                                                    value={plan.value}
                                                    checked={planType === plan.value}
                                                    onChange={(event) => setPlanType(event.target.value as PlanType)}
                                                />
                                                <div className="stack" style={{ gap: 2 }}>
                                                    <span className="text-sm font-medium">{plan.title}</span>
                                                    <span className="text-xs text-slate-500">{plan.price} – {plan.description}</span>
                                                </div>
                                            </label>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        )}
                        <Button disabled={tab === "sign-in" ? signInForm.formState.isSubmitting : signUpForm.formState.isSubmitting}>
                            {tab === "sign-in" ? "Sign In" : "Create Account"}
                        </Button>
                        {tab === "sign-up" && signUpForm.formState.errors.root?.message && (
                            <div className="helper" role="alert">{signUpForm.formState.errors.root.message}</div>
                        )}
                        <div className="helper" style={{ textAlign: "center" }}>
                            {tab === "sign-in" ? "Forgot password? (add later)" : "By continuing you agree to the Terms"}
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}
