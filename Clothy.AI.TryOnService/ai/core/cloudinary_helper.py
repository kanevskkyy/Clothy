import cloudinary.uploader


async def process_clothe_cloudinary(url: str) -> str:
    result = cloudinary.uploader.upload(
        url,
        background_removal='cloudinary_ai',
        format='png'
    )
    return result['secure_url']