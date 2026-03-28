import apiClient from "./client.ts";

interface ITryOnResponse {
    outputImageUrl: string;
    generatedTime: number;
    orderId: string;
}

export const tryOnService = {
    async tryOn(personImage: File, clotheImageUrl: string): Promise<ITryOnResponse> {
        const formData = new FormData();
        formData.append('personImage', personImage);
        formData.append('clotheImageUrl', clotheImageUrl);

        const response = await apiClient.post<ITryOnResponse>('/api/try-on', formData, {
            timeout: 60000,
        });

        return response.data;
    },
};