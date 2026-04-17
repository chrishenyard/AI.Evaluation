$buildDate = (Get-Date).ToUniversalTime().ToString("o")

docker compose build --build-arg BUILD_DATE=$buildDate
docker compose create PromptEval
docker compose start PromptEval
