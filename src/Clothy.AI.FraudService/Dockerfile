FROM python:3.11-slim

# xgboost потребує OpenMP (libgomp), якого немає в slim-образі
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgomp1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# models/ не в git: перед збіркою скопіюйте файли моделей (див. README.md)
COPY src ./src
COPY models ./models

ENV MODELS_DIR=models \
    MODEL_VARIANT=main \
    FLAG_RATE=0.01 \
    STORE_BACKEND=sqlite \
    DB_PATH=/data/history.db \
    RING_SIGNAL=on \
    LOG_LEVEL=INFO

# Історія SQLite переживає перезапуск, якщо /data змонтовано як volume
RUN mkdir -p /data
VOLUME /data

EXPOSE 8097

HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
    CMD python -c "import urllib.request,sys; sys.exit(0 if urllib.request.urlopen('http://127.0.0.1:8097/health', timeout=2).status == 200 else 1)"

CMD ["uvicorn", "src.service:create_app", "--factory", "--host", "0.0.0.0", "--port", "8097"]
