from try_on import AppFactory, Config

app = AppFactory().create_app(
    Config()
)

