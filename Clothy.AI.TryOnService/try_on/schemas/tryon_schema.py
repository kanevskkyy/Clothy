from pydantic import BaseModel, field_validator

def to_camel(string: str) -> str:
    parts = string.split('_')
    return parts[0] + ''.join(word.capitalize() for word in parts[1:])


class TryOnRequest(BaseModel):
    person_image: bytes
    clothe_image_url: str

    @field_validator('clothe_image_url')
    def validate_clothe_image(cls, value: str) -> str:
        if len(value) == 0:
            raise ValueError('Clothe image url cannot be empty')
        return value

    model_config = {
        'alias_generator': to_camel,
        'populate_by_name': True
    }


class TryOnResponse(BaseModel):
    output_image_url: str
    generated_time: int
    order_id: str

    model_config = {
        'alias_generator': to_camel,
        'populate_by_name': True
    }