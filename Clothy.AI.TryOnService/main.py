from try_on import AppFactory, Config
import uvicorn

app = AppFactory().create_app(
    Config()
)

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8081, reload=True)