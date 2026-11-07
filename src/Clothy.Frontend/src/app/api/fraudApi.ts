import axios from "axios";
import apiClient from "./client.ts";

export interface IFraudTopFeature {
    feature: string;
    value: number | null;
    contribution: number;
}

export interface IFraudRingAlert {
    other_cards_30d: number | null;
    share_new: number | null;
}

/** What the risk badge needs; both IFraudTxRecord and IFraudDecision have it. */
export interface IFraudRiskSummary {
    score: number | null;
    flag: boolean | null;
    ring_alert: IFraudRingAlert | null;
}

/** GET /api/fraud/tx/{orderId}: the decision stored by the fraud service (txid = order id). */
export interface IFraudTxRecord {
    txid: string;
    ts: number;
    amount: number;
    fraud_label_at: number | null;
    variant: string | null;
    score: number | null;
    flag: boolean | null;
    threshold: number | null;
    ring_alert: IFraudRingAlert | null;
    top_features: IFraudTopFeature[] | null;
}

export interface IFraudDecision {
    txid: string;
    ts: number;
    score: number;
    flag: boolean;
    threshold: number;
    variant: string;
    fraud_label_at: number | null;
    top_features: IFraudTopFeature[] | null;
    ring_alert: IFraudRingAlert | null;
}

// Admin only: the gateway checks the Admin role on every /api/fraud route.
export const fraudApi = {
    /** null when the order was not scored (404: service was down, late webhook, or not paid by card). */
    getOrderRiskAsync: async (orderId: string): Promise<IFraudTxRecord | null> => {
        try {
            const { data } = await apiClient.get<IFraudTxRecord>(`/api/fraud/tx/${orderId}`, {skipNotFoundRedirect: true});
            return data;
        } catch (error) {
            if (axios.isAxiosError(error) && error.response?.status === 404) return null;
            throw error;
        }
    },

    getDecisionsAsync: async (flagged?: boolean, limit = 500): Promise<IFraudDecision[]> => {
        let url = `/api/fraud/decisions?limit=${limit}`;
        if (flagged !== undefined) url += `&flagged=${flagged}`;

        const { data } = await apiClient.get<{ items: IFraudDecision[] }>(url);
        return data.items;
    },
};
