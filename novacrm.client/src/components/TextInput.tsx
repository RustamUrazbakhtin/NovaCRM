import type { InputHTMLAttributes } from "react";
import { forwardRef } from "react";

type Props = InputHTMLAttributes<HTMLInputElement> & { error?: string };

export const TextInput = forwardRef<HTMLInputElement, Props>(
    ({ error, ...rest }, ref) => (
        <div className="stack">
            <input ref={ref} className="input" {...rest} />
            {error && <div className="helper">{error}</div>}
        </div>
    )
);
TextInput.displayName = "TextInput";
