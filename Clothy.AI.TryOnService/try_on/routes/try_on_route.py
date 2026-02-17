# router.py
from fastapi import HTTPException, APIRouter, UploadFile, File, Form

from try_on import Config
from try_on.exceptions import LightXAPIException
from try_on.schemas import TryOnRequest, TryOnResponse
from try_on.services.lightx_service import LightXService

router = APIRouter(prefix='/try-on', tags=['try-on'])

@router.post('', response_model=TryOnResponse)
async def try_on(
    person_image: UploadFile = File(..., alias='personImage'),
    clothe_image_url: str = Form(..., alias='clotheImageUrl')
) -> TryOnResponse:
    light_x_service = LightXService(Config.LIGHT_X_API_KEY)

    try:
        person_image_bytes = await person_image.read()

        request = TryOnRequest(person_image=person_image_bytes,clothe_image_url=clothe_image_url)
        response = await light_x_service.create_try_on(request)
        return response
    except TimeoutError as e:
        raise HTTPException(status_code=504, detail=str(e))
    except LightXAPIException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f'Internal service error: {e}')