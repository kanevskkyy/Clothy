from typing import Union
import asyncio
from concurrent.futures import ThreadPoolExecutor

import cloudinary.uploader

executor = ThreadPoolExecutor()


async def process_clothe_cloudinary(source: Union[str, bytes]) -> str:
    loop = asyncio.get_event_loop()

    result = await loop.run_in_executor(
        executor,
        lambda: cloudinary.uploader.upload(
            source,
            background_removal='cloudinary_ai',
            format='png'
        )
    )

    return result['secure_url']