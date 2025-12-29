from fastapi import HTTPException, APIRouter

from ai import Config
from ai.exceptions import LightXAPIException
from ai.schemas import TryOnRequest, TryOnResponse
from ai.services.lightx_service import LightXService

router = APIRouter(prefix='/try-on', tags=['try-on'])

@router.post("/", response_model=TryOnResponse)
async def try_on(request: TryOnRequest) -> TryOnResponse:
    light_x_service = LightXService(Config.LIGHT_X_API_KEY)

    try:
        response = await light_x_service.create_try_on(request)
        return response
    except TimeoutError as e:
        raise HTTPException(status_code=504, detail=str(e))
    except LightXAPIException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f'Internal service error: {e}')
