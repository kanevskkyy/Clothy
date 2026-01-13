import asyncio
import logging

import httpx

from try_on.core.cloudinary_helper import process_clothe_cloudinary
from try_on.exceptions import LightXAPIException
from try_on.schemas import TryOnRequest, TryOnResponse

logger = logging.getLogger(__name__)


class LightXService:
    def __init__(self, api_key: str) -> None:
        self.api_key = api_key
        self.base_url = 'https://api.lightxeditor.com/external/api/v2'

    async def create_try_on(self, req: TryOnRequest) -> TryOnResponse:
        image_base64 = await process_clothe_cloudinary(req.person_image)
        clothe_base64 = await process_clothe_cloudinary(req.clothe_image_url)

        body_request = {
            'imageUrl': image_base64,
            'styleImageUrl': clothe_base64,
        }

        async with httpx.AsyncClient(timeout=30.0) as client:
            try:
                response = await client.post(
                    f'{self.base_url}/aivirtualtryon/',
                    headers={
                        'x-api-key': self.api_key,
                        'Content-Type': 'application/json',
                    },
                    json=body_request,
                )

                response.raise_for_status()
                data = response.json()
                logger.info(f'Initial response: {data}')

                if not data or 'body' not in data or 'orderId' not in data.get('body', {}):
                    raise LightXAPIException(f'Unexpected response structure: {data}')

                order_id = data['body']['orderId']
                max_retries = data['body'].get('maxRetriesAllowed', 30)

            except httpx.HTTPStatusError as e:
                logger.error(f'HTTP error during initial request: {e.response.status_code} - {e.response.text}')
                raise LightXAPIException(f'HTTP error: {e.response.status_code}')
            except Exception as e:
                logger.error(f'Error during initial request: {e}')
                raise

            for i in range(max_retries):
                await asyncio.sleep(5)

                try:
                    status_response = await client.post(
                        f'{self.base_url}/order-status/',
                        headers={
                            'x-api-key': self.api_key,
                            'Content-Type': 'application/json',
                        },
                        json={
                            'orderId': order_id,
                        }
                    )

                    status_response.raise_for_status()
                    status_data = status_response.json()

                    if not status_data:
                        logger.warning(f'Empty response on attempt {i+1}/{max_retries}')
                        continue

                    logger.info(f'Status check {i+1}/{max_retries}: {status_data}')

                    message = status_data.get('message')
                    body = status_data.get('body') or {}

                    if message == 'SUCCESS' and body.get('output') is not None:
                        return TryOnResponse(
                            output_image_url=body.get('output'),
                            generated_time=int(body.get('generatedTime', 0)),
                            order_id=body.get('orderId', order_id),
                        )

                    status = status_data.get('status')
                    if status == 'FAIL' or message == 'FAIL':
                        error_msg = status_data.get('error') or body.get('error') or 'Unknown error'
                        raise LightXAPIException(f'LightX API failed: {error_msg}')

                except httpx.HTTPStatusError as e:
                    logger.error(f'HTTP error during status check: {e.response.status_code} - {e.response.text}')
                    if i == max_retries - 1:
                        raise LightXAPIException(f'HTTP error: {e.response.status_code}')
                    continue
                except Exception as e:
                    logger.error(f'Error during status check {i+1}: {e}')
                    if i == max_retries - 1:
                        raise
                    continue

            raise TimeoutError('LightX try-on timed out')