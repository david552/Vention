1) Docker Build - docker build -f Vention.API/Dockerfile -t vention-api .

2) Docker Run - docker run --rm -d -p 5000:8080 --name my-vention-api -e "ASPNETCORE_ENVIRONMENT=Development" -e "CryptoSettings__PasswordPepper=12345" vention-api

3) Docker Stop - docker stop my-vention-api