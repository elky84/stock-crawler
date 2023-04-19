[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
![Made with](https://img.shields.io/badge/made%20with-.NET7-brightgreen.svg)
![Made with](https://img.shields.io/badge/made%20with-JavaScript-blue.svg)
![Made with](https://img.shields.io/badge/made%20with-MongoDB-red.svg)

[![Publish Docker image](https://github.com/elky84/stock-crawler/actions/workflows/publish_docker.yml/badge.svg)](https://github.com/elky84/stock-crawler/actions/workflows/publish_docker.yml)

![GitHub forks](https://img.shields.io/github/forks/elky84/stock-crawler.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/stock-crawler.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/stock-crawler.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/stock-crawler.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/stock-crawler.svg)

# stock-crawler

## 소개
* .NET 7, ASP NET CORE 3를 기반으로 작성되었습니다.
* 웹 크롤러로는 [abot2](https://github.com/sjdirect/abot) 를 사용했습니다.
* DB로는 mongoDB를 사용합니다.
* API는 swagger를 통해 확인하셔도 좋지만, http 폴더 안에 예제가 포함되어있습니다.

## 사용법
* MongoDB 설정 (Server 프로젝트)
  * `MONGODB_CONNECTION` 환경 변수에 `MONGODB 커넥션 문자열` 입력
* 선택적 MongoDB 데이터베이스
  * 기본 값은 `stock-crawler`
  * `MONGODB_DATABASE` 환경 변수 사용시 override
* 환경 변수 미사용시, appSettings.[환경].json 파일에 있는 값을 사용합니다. (환경에 맞는 파일 미제공시, appSettings.json 의 값을 그대로 이용)

## 주요 기능
* 네이버 주식에서 데이터 긁어오기 (크롤링), 모의투자 (자동 매수, 매도 기능 포함) 등의 기능
* 알고리즘별 추천 종목 선정 기능

## 주의 사항
* 모든 기능은 참고용이며, 모의 기능만 포함하고 있습니다.
* 추천 종목을 포함한 모든 기능은 참고용으로만 사용하시길 권장드립니다.
* 크롤링 자체도 무거운편이지만, async로 수많은 동작을 병렬로 수행하게 구현되어있습니다. 고사양 PC 혹은 서버에서 가동 하시길 권장드립니다.

## 각종 API 예시
* VS Code의 RestClient Plugin의 .http 파일용으로 작성
  * <https://marketplace.visualstudio.com/items?itemName=humao.rest-client>
* .http 파일 경로
  * <https://github.com/elky84/stock-crawler/tree/master/Http>
* 해당 경로 아래에 .vscode 폴더에 settings.json.sample을 복사해, settings.json으로 변경하면, VSCode로 해당 기능 오픈시 환경에 맞는 URI로 호출 가능하게 됨
  * <https://github.com/elky84/stock-crawler/blob/master/Http/.vscode/settings.json.sample>
* Swagger로 확인해도 무방함
  * <http://localhost:5000/swagger/index.html>
