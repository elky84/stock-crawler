set DOCKER_REGISTRY=%1

docker build -t %DOCKER_REGISTRY%/stock-crawler -f ./Server/Dockerfile .
docker push %DOCKER_REGISTRY%/stock-crawler
