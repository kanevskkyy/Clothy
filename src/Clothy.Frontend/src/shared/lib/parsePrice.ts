export const parsePrice = (priceString: string): number => {
    return Number(priceString.replace(',', '.'));
};