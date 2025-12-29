from decouple import config

class Config:
    LIGHT_X_API_KEY = config('LIGHT_X_API_KEY', default=None)
    ALLOWED_ORIGINS = config('ALLOWED_ORIGINS', default='*').split(',')

    CLOUDINARY_CLOUD_NAME = config('CLOUDINARY_CLOUD_NAME', default=None)
    CLOUDINARY_API_KEY = config('CLOUDINARY_API_KEY', default=None)
    CLOUDINARY_API_SECRET = config('CLOUDINARY_API_SECRET', default=None)