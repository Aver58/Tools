# reference https://github.com/docker/awesome-compose/tree/master/flask
FROM --platform=$BUILDPLATFORM python:3.10-slim AS builder

WORKDIR /app

EXPOSE 8081

COPY requirements.txt /app
RUN --mount=type=cache,target=/root/.cache/pip \
    pip3 install -r requirements.txt

COPY . /app

ENTRYPOINT ["python3"]
CMD ["main.py"]