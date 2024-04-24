FROM python:3.8-slim-buster

WORKDIR /app

ADD . /app

RUN pip install --no-cache-dir -r requirements.txt

EXPOSE 8000

# Set the host to 0.0.0.0 to listen on all interfaces
ENV HOST=0.0.0.0
ENV PORT=8000

CMD uvicorn Model_Upload:app --host $HOST --port $PORT