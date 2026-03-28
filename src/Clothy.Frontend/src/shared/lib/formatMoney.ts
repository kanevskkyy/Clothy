export const formatMoney = (value: number) => {
    if (value === undefined || value === null) return "0.00";
    return value.toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    });
};