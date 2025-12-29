class APIException(Exception):
    status_code = 400
    message = 'Something went wrong'

    def __init__(self, message: str | None = None, status_code: int | None = None):
        if message:
            self.message = message
        if status_code:
            self.status_code = status_code
        super().__init__(self.message)


class LightXAPIException(APIException):
    status_code = 400
    message = 'Invalid photos for fitting'