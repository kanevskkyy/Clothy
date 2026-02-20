import type { ZodError } from 'zod';

export function getZodFieldErrors<T>(error: ZodError<T>): Partial<Record<keyof T, string>> {
    const fieldErrors: Partial<Record<keyof T, string>> = {};
    error.issues.forEach((issue) => {
        const field = issue.path[0] as keyof T;
        if (field !== undefined) {
            fieldErrors[field] = issue.message;
        }
    });
    return fieldErrors;
}