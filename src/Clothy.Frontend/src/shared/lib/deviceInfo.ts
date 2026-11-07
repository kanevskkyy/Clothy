// Basic device data for fraud scoring, in the format of the IEEE-CIS fields the model knows:
// DeviceType (desktop/mobile), DeviceInfo, id_30 (OS), id_31 (browser), id_33 (screen).
// Sent once with the payment request and shown to admins only.

export interface IPaymentDeviceDTO {
    deviceType?: string;
    deviceInfo?: string;
    os?: string;
    browser?: string;
    screen?: string;
}

const WINDOWS_VERSIONS: Record<string, string> = {
    "10.0": "Windows 10",
    "6.3": "Windows 8.1",
    "6.2": "Windows 8",
    "6.1": "Windows 7",
};

const majorMinor = (version: string): string => {
    const parts = version.split(".");
    return `${parts[0]}.${parts[1] ?? "0"}`;
};

const detectOs = (ua: string): { os?: string; deviceInfo?: string } => {
    let m = ua.match(/Windows NT (\d+\.\d+)/);
    if (m) return {os: WINDOWS_VERSIONS[m[1]] ?? "Windows", deviceInfo: "Windows"};

    m = ua.match(/(?:iPhone|iPad|iPod).*? OS (\d+(?:_\d+)*)/);
    if (m) return {os: `iOS ${m[1].replace(/_/g, ".")}`, deviceInfo: "iOS Device"};

    m = ua.match(/Android (\d+(?:\.\d+)*)(?:; ([^;)]+))?/);
    if (m) {
        // modern browsers send the reduced user agent with the model "K"
        const model = m[2]?.trim();
        return {os: `Android ${m[1]}`, deviceInfo: model && model !== "K" ? model : undefined};
    }

    m = ua.match(/Mac OS X (\d+(?:[_.]\d+)*)/);
    if (m) return {os: `Mac OS X ${m[1].replace(/\./g, "_")}`, deviceInfo: "MacOS"};

    if (/Linux/.test(ua)) return {os: "Linux", deviceInfo: "Linux"};
    return {};
};

const detectBrowser = (ua: string, isMobile: boolean): string | undefined => {
    let m = ua.match(/SamsungBrowser\/(\d+(?:\.\d+)?)/);
    if (m) return `samsung browser ${majorMinor(m[1])}`;

    m = ua.match(/Edg(?:A|iOS)?\/(\d+(?:\.\d+)?)/);
    if (m) return `edge ${majorMinor(m[1])}`;

    m = ua.match(/OPR\/(\d+(?:\.\d+)?)/);
    if (m) return `opera ${majorMinor(m[1])}`;

    m = ua.match(/CriOS\/(\d+(?:\.\d+)?)/);
    if (m) return `chrome ${majorMinor(m[1])} for ios`;

    m = ua.match(/(?:Firefox|FxiOS)\/(\d+(?:\.\d+)?)/);
    if (m) return `firefox ${majorMinor(m[1])}`;

    m = ua.match(/Chrome\/(\d+(?:\.\d+)?)/);
    if (m) return /Android/.test(ua) ? `chrome ${majorMinor(m[1])} for android` : `chrome ${majorMinor(m[1])}`;

    m = ua.match(/Version\/(\d+(?:\.\d+)?).*Safari/);
    if (m) return isMobile ? `mobile safari ${majorMinor(m[1])}` : `safari ${majorMinor(m[1])}`;

    return undefined;
};

const detectScreen = (): string | undefined => {
    if (typeof window === "undefined" || !window.screen) return undefined;
    const dpr = window.devicePixelRatio || 1;
    const w = Math.round(window.screen.width * dpr);
    const h = Math.round(window.screen.height * dpr);
    if (!w || !h) return undefined;
    // IEEE-CIS writes the larger side first, e.g. 1334x750
    return `${Math.max(w, h)}x${Math.min(w, h)}`;
};

export const getPaymentDeviceInfo = (): IPaymentDeviceDTO | undefined => {
    try {
        const ua = navigator.userAgent;
        const isMobile = /Mobi|Android|iPhone|iPad|iPod/.test(ua);
        const {os, deviceInfo} = detectOs(ua);
        return {
            deviceType: isMobile ? "mobile" : "desktop",
            deviceInfo,
            os,
            browser: detectBrowser(ua, isMobile),
            screen: detectScreen(),
        };
    } catch {
        return undefined;
    }
};
