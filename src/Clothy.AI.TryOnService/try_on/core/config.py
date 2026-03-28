from decouple import config

class Config:
    LIGHT_X_API_KEY = config('LIGHT_X_API_KEY', default=None)
    ALLOWED_ORIGINS = config('FRONTEND__URL', default='*').split(',')

    CLOUDINARY_CLOUD_NAME = config('CLOUDINARYSETTINGS__CLOUDNAME', default=None)
    CLOUDINARY_API_KEY = config('CLOUDINARYSETTINGS__APIKEY', default=None)
    CLOUDINARY_API_SECRET = config('CLOUDINARYSETTINGS__APISECRET', default=None)