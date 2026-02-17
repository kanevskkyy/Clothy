import { jwtDecode } from "jwt-decode";

export interface IJwtPayload {
    email?: string;
    email_verified?: boolean;
    family_name?: string;
    given_name?: string;
    name?: string;
    phone_number?: string;
    photo_url?: string;
    preferred_username?: string;
    roles?: string[];
}

export const decodeJwt = (accessToken: string): IJwtPayload => {
    return jwtDecode<IJwtPayload>(accessToken);
};