import logging

import cloudinary
from fastapi import FastAPI
from starlette.middleware.cors import CORSMiddleware

from ai.core import Config
from ai.routes.try_on_route import router


class AppFactory:
    @staticmethod
    def create_app(config: Config) -> FastAPI:
        app = FastAPI(
            title='Clothy AI Try-On API',
            description='API for virtual try-on',
            version='1.0.0',
        )

        app.add_middleware(
            CORSMiddleware,
            allow_origins=config.ALLOWED_ORIGINS,
            allow_credentials=True,
            allow_methods=["*"],
            allow_headers=["*"],
        )

        app.state.config = config
        app.include_router(router)
        logging.basicConfig(level=logging.INFO)

        cloudinary.config(
            cloud_name=config.CLOUDINARY_CLOUD_NAME,
            api_key=config.CLOUDINARY_API_KEY,
            api_secret=config.CLOUDINARY_API_SECRET,
        )

        return app
