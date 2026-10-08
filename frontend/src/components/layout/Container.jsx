import { cn } from "cn";

export function Container({ className, children, ...props }) {
  return (
    <div className={cn("mx-auto w-full max-w-7xl px-4 sm:px-6", className)} {...props}>
      {children}
    </div>
  );
}
