export const formatMoney = (value: number) =>
    value.toLocaleString("uk-UA", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    });